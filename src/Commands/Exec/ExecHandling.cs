using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cicee.CiEnv;
using LanguageExt.Common;

namespace Cicee.Commands.Exec
{
  public static class ExecHandling
  {
    private const string CiDirectoryName = Conventions.CiDirectoryName;
    private const string ProjectRoot = "PROJECT_ROOT";
    private const string LibRoot = "LIB_ROOT";
    private const string CiCommand = "CI_COMMAND";
    private const string CiEntrypoint = "CI_ENTRYPOINT";
    private const string CiExecImage = "CI_EXEC_IMAGE";
    private const string CiInitScript = "CI_ENV_INIT";

    /// <summary>
    ///   Build context for ci-exec service.
    /// </summary>
    private const string CiExecContext = "CI_EXEC_CONTEXT";

    private const string CiceeExecScriptName = "cicee-exec.sh";

    public static Result<ProcessStartInfo> CreateProcessStartInfo(
      ExecDependencies dependencies,
      ExecRequestContext execRequestContext)
    {
      var ciceeExecPath = Path.Combine(dependencies.GetLibraryRootPath(), CiceeExecScriptName);
      return dependencies.EnsureFileExists(ciceeExecPath)
        .MapLeft(
          exception => exception is FileNotFoundException
            ? new BadRequestException($"Failed to find library file: {ciceeExecPath}")
            : exception
        )
        .Bind(validatedCiceeExecPath =>
        {
          // TODO: See if the path can be inferred correctly, rather than hacking it into Linux assumptions.
          var ciceeExecLinuxPath = NormalizeToLinuxPath(validatedCiceeExecPath);

          return ProcessHelpers.TryCreateBashProcessStartInfo(
            GetExecEnvironment(dependencies, execRequestContext),
            ciceeExecLinuxPath
          );
        });
    }

    public static async Task<Result<ExecRequestContext>> HandleAsync(
      ExecDependencies dependencies,
      ExecRequest request
    )
    {
      dependencies.StandardOutWriteLine("Beginning exec...\n");
      dependencies.StandardOutWriteLine($"Project root: {request.ProjectRoot}");
      dependencies.StandardOutWriteLine($"Entrypoint  : {request.Entrypoint}");
      dependencies.StandardOutWriteLine($"Command     : {request.Command}");

      return await TryCreateRequestContext(dependencies, request)
        .Bind(execContext => ValidateContext(dependencies, execContext))
        .Map(context => DisplayExecContext(dependencies, context))
        .BindAsync(execContext => TryExecute(dependencies, execContext));
    }

    private static string CreateCiDockerfilePath(ExecDependencies dependencies, ExecRequest request)
    {
      return Path.Combine(request.ProjectRoot, CiDirectoryName, "Dockerfile");
    }

    public static Result<ExecRequestContext> TryCreateRequestContext(ExecDependencies dependencies, ExecRequest request)
    {
      return
        dependencies.EnsureDirectoryExists(request.ProjectRoot)
          .Bind(validatedProjectRoot =>
            ProjectMetadataLoader.TryLoad(
                dependencies.EnsureDirectoryExists,
                dependencies.EnsureFileExists,
                dependencies.TryLoadFileString,
                validatedProjectRoot
              )
              .Match(
                value => new Result<ProjectMetadata>(value),
                loadFailure =>
                  // We failed to load metadata, but we know that the project root exists.
                  new ProjectMetadata {Name = "unknown-project", Title = "Unknown Project", Version = "0.0.0"})
          )
          .Bind(projectMetadata => Require.AsResult.NotNullOrWhitespace(request.Image)
            .BindLeft(_ => dependencies.EnsureFileExists(CreateCiDockerfilePath(dependencies, request)))
            .MapLeft(exception =>
              new BadRequestException(
                $"Image argument was not provided and '{CreateCiDockerfilePath(dependencies, request)}; does not exist.",
                exception)
            )
            .Map(_ => projectMetadata)
          )
          .Map(projectMetadata =>
            {
              var dockerfile = dependencies
                .EnsureFileExists(Path.Combine(request.ProjectRoot, CiDirectoryName, "Dockerfile"))
                .Match(file => (string?)file, _ => (string?)null);
              return new ExecRequestContext(request.ProjectRoot, projectMetadata, request.Command, request.Entrypoint,
                GetEnvironmentInitializationScriptPath(dependencies, request),
                dockerfile,
                request.Image
              );
            }
          );
    }

    private static ExecRequestContext DisplayExecContext(
      ExecDependencies dependencies,
      ExecRequestContext execRequestContext
    )
    {
      void WriteEnvironmentVariables(IReadOnlyDictionary<string, string> environmentDisplay)
      {
        var width = environmentDisplay.Keys.Max(value => value.Length) + 1;
        foreach (var (key, value) in environmentDisplay.OrderBy(kvp => kvp.Key))
        {
          dependencies.StandardOutWriteLine($"  {key.PadRight(width, paddingChar: ' ')}: {value}");
        }
      }

      void DisplayProjectEnvironmentValues()
      {
        var environmentDisplay =
          ProjectEnvironmentHelpers.GetEnvironmentDisplay(dependencies.GetEnvironmentVariables, execRequestContext);
        dependencies.StandardOutWriteLine("CI Environment:");
        if (environmentDisplay.Any())
        {
          WriteEnvironmentVariables(environmentDisplay);
        }
        else
        {
          dependencies.StandardOutWriteLine("  No CI environment variables defined.");
        }
      }

      void DisplayExecEnvironmentValues()
      {
        var environmentDisplay = GetExecEnvironment(dependencies, execRequestContext);
        dependencies.StandardOutWriteLine("CICEE Execution Environment:");
        WriteEnvironmentVariables(environmentDisplay);
      }


      DisplayProjectEnvironmentValues();
      DisplayExecEnvironmentValues();

      return execRequestContext;
    }

    private static string? GetEnvironmentInitializationScriptPath(ExecDependencies dependencies, ExecRequest request)
    {
      // TODO: Extract these env paths into default value within application configuration.
      var envPaths = new[] {".env", "ci.env", "env.sh", "project.sh"};

      return envPaths
        .Select(relativeFileName => Path.Combine(request.ProjectRoot, CiDirectoryName, relativeFileName))
        .FirstOrDefault(relativeFileName =>
          dependencies.EnsureFileExists(relativeFileName).Match(_ => true, _ => false)
        );
    }

    private static IReadOnlyDictionary<string, string> GetExecEnvironment(
      ExecDependencies dependencies,
      ExecRequestContext context
    )
    {
      var environment =
        new Dictionary<string, string>
        {
          [CiExecContext] = NormalizeToLinuxPath(Path.Combine(context.ProjectRoot, CiDirectoryName)),
          [ProjectRoot] = NormalizeToLinuxPath(context.ProjectRoot),
          [LibRoot] = NormalizeToLinuxPath(dependencies.GetLibraryRootPath())
        };

      void ConditionallyAdd(string key, string? possibleValue)
      {
        if (!string.IsNullOrWhiteSpace(possibleValue))
        {
          environment[key] = possibleValue!;
        }
      }

      ConditionallyAdd(CiCommand, context.Command);
      ConditionallyAdd(CiEntrypoint, context.Entrypoint);
      ConditionallyAdd(
        CiInitScript,
        context.EnvironmentInitializationScriptPath != null
          ? NormalizeToLinuxPath(context.EnvironmentInitializationScriptPath)
          : null
      );
      ConditionallyAdd(CiExecImage, context.Image);

      return environment;
    }

    private static string NormalizeToLinuxPath(string path)
    {
      static string WindowsToLinuxPath(string path)
      {
        var driveAndPath = path.Split(":\\");
        return $"/{driveAndPath[0].ToLowerInvariant()}/{driveAndPath[1].Replace(oldChar: '\\', newChar: '/')}";
      }

      return path.Contains(":\\") ? WindowsToLinuxPath(path) : path;
    }


    private static async Task<Result<ExecRequestContext>> TryExecute(
      ExecDependencies dependencies,
      ExecRequestContext execRequestContext
    )
    {
      return (
          await CreateProcessStartInfo(dependencies, execRequestContext)
            .BindAsync(dependencies.ProcessExecutor)
        )
        .Map(_ => execRequestContext);
    }

    private static Result<ExecRequestContext> ValidateContext(
      ExecDependencies dependencies,
      ExecRequestContext execRequestContext
    )
    {
      static Result<ExecRequestContext> RequireStartupCommand(ExecRequestContext context)
      {
        // Require either a command
        return Require.AsResult.NotNullOrWhitespace(context.Command)
          .BindLeft(missingCommandException =>
            // ... or an entrypoint
            Require.AsResult.NotNullOrWhitespace(context.Entrypoint)
          )
          .MapLeft(exception =>
            new BadRequestException("At least one of command or entrypoint must be provided.", exception)
          )
          .Map(_ => context);
      }

      Result<ExecRequestContext> RequireProjectRoot(ExecRequestContext contextWithStartupCommand)
      {
        return dependencies.EnsureDirectoryExists(contextWithStartupCommand.ProjectRoot)
          .MapLeft(exception => exception is DirectoryNotFoundException
            ? new BadRequestException(
              $"Project root '{contextWithStartupCommand.ProjectRoot}' cannot be found.")
            : exception)
          .Map(projectRoot => contextWithStartupCommand);
      }

      return RequireStartupCommand(execRequestContext)
        .Bind(RequireProjectRoot)
        .Bind(execContext =>
          ProjectEnvironmentHelpers.ValidateEnvironment(dependencies.GetEnvironmentVariables, execContext)
        );
    }
  }
}
