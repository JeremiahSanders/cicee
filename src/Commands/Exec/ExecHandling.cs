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
    private const string CiDirectoryName = "ci";
    private const string ProjectRoot = "PROJECT_ROOT";
    private const string LibRoot = "LIB_ROOT";
    private const string CiCommand = "CI_COMMAND";
    private const string CiEntrypoint = "CI_ENTRYPOINT";
    private const string CiExecDockerfile = "CI_EXEC_DOCKERFILE";
    private const string CiExecImage = "CI_EXEC_IMAGE";
    private const string CiInitScript = "CI_ENV_INIT";
    private const string CiceeExecScriptName = "cicee-exec.sh";

    public static Result<ProcessStartInfo> CreateProcessStartInfo(
      ExecDependencies dependencies,
      ExecRequestContext execRequestContext)
    {
      var libPath = dependencies.GetLibraryRootPath();
      var ciceeExecPath = Path.Combine(libPath, CiceeExecScriptName);
      return dependencies.EnsureFileExists(ciceeExecPath)
        .MapLeft(
          exception => exception is FileNotFoundException
            ? new BadRequestException($"Failed to find library file: {ciceeExecPath}")
            : exception
        )
        .Bind(validatedCiceeExecPath =>
        {
          var environment = GetExecEnvironment(dependencies, execRequestContext, libPath);

          // TODO: See if the path can be inferred correctly, rather than hacking it into Linux assumptions.
          var ciceeExecLinuxPath = NormalizeToLinuxPath(validatedCiceeExecPath);

          return ProcessHelpers.TryCreateBashProcessStartInfo(environment, ciceeExecLinuxPath);
        });
    }

    public static async Task<Result<ExecRequestContext>> HandleAsync(
      ExecDependencies dependencies,
      ExecRequest request
    )
    {
      dependencies.StandardOutWriteLine($"Project root: {request.ProjectRoot}");
      dependencies.StandardOutWriteLine($"Entrypoint  : {request.Entrypoint}");
      dependencies.StandardOutWriteLine($"Command     : {request.Command}");

      return await TryCreateRequestContext(dependencies, request)
        .Bind(execContext => ValidateContext(dependencies, execContext))
        .Map(context => DisplayExecContext(dependencies, context))
        .BindAsync(execContext => TryExecute(dependencies, execContext));
    }

    public static Result<ExecRequestContext> TryCreateRequestContext(ExecDependencies dependencies, ExecRequest request)
    {
      return ProjectMetadataLoader.TryLoad(
          dependencies.EnsureDirectoryExists,
          dependencies.EnsureFileExists,
          dependencies.TryLoadFileString,
          request.ProjectRoot
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
      dependencies.StandardOutWriteLine("Project metadata loaded.");
      dependencies.StandardOutWriteLine("Environment:");
      var environmentDisplay =
        ProjectEnvironmentHelpers.GetEnvironmentDisplay(dependencies.GetEnvironmentVariables, execRequestContext);
      if (environmentDisplay.Any())
      {
        foreach (var (key, value) in environmentDisplay)
        {
          dependencies.StandardOutWriteLine($"  {key}: {value}");
        }
      }
      else
      {
        dependencies.StandardOutWriteLine("  No environment defined.");
      }

      return execRequestContext;
    }

    private static string? GetEnvironmentInitializationScriptPath(ExecDependencies dependencies, ExecRequest request)
    {
      // TODO: Extract these env paths into default value within application configuration.
      var envPaths = new[] {".env", "ci.env", "env.sh", "project.sh"};

      return envPaths
        .Select(relativeFileName => Path.Combine(request.ProjectRoot, CiDirectoryName, relativeFileName))
        .FirstOrDefault(relativeFileName =>
          dependencies.EnsureFileExists(relativeFileName)
            .Match(
              _ => true,
              _ =>
              {
                dependencies.StandardOutWriteLine(
                  $"No project environment initialization script found in '{CiDirectoryName}' directory.");
                return false;
              })
        );
    }

    private static IReadOnlyDictionary<string, string> GetExecEnvironment(
      ExecDependencies dependencies,
      ExecRequestContext context,
      string libPath
    )
    {
      var environment =
        new Dictionary<string, string>
        {
          [ProjectRoot] = NormalizeToLinuxPath(context.ProjectRoot), [LibRoot] = NormalizeToLinuxPath(libPath)
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
      ConditionallyAdd(CiExecDockerfile, context.Dockerfile);
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
        return string.IsNullOrWhiteSpace(context.Command) &&
               string.IsNullOrWhiteSpace(context.Entrypoint)
          ? new Result<ExecRequestContext>(
            new BadRequestException("At least one of command or entrypoint must be provided."))
          : new Result<ExecRequestContext>(context);
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
