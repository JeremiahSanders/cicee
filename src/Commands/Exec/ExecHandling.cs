using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Cicee.CiEnv;
using Cicee.Dependencies;

using LanguageExt.Common;

namespace Cicee.Commands.Exec;

public static class ExecHandling
{
  private const string CiDirectoryName = Conventions.CiDirectoryName;
  private const string ProjectName = "PROJECT_NAME";
  private const string ProjectRoot = "PROJECT_ROOT";
  private const string LibRoot = "LIB_ROOT";
  private const string CiCommand = "CI_COMMAND";
  private const string CiEntrypoint = "CI_ENTRYPOINT";
  private const string CiExecImage = "CI_EXEC_IMAGE";

  /// <summary>
  ///   Build context for ci-exec service.
  /// </summary>
  private const string CiExecContext = "CI_EXEC_CONTEXT";

  private const string CiceeExecScriptName = "cicee-exec.sh";

  public static Result<ProcessStartInfo> CreateProcessStartInfo(CommandDependencies dependencies,
    ExecRequestContext execRequestContext)
  {
    string ciceeExecPath = dependencies.CombinePath(dependencies.GetLibraryRootPath(), CiceeExecScriptName);
    return dependencies.EnsureFileExists(ciceeExecPath).MapFailure(
      exception => exception is FileNotFoundException
        ? new BadRequestException($"Failed to find library file: {ciceeExecPath}")
        : exception
    ).Bind(
      validatedCiceeExecPath =>
      {
        string ciceeExecLinuxPath = Io.NormalizeToLinuxPath(validatedCiceeExecPath);

        return ProcessHelpers.TryCreateBashProcessStartInfo(
          GetExecEnvironment(dependencies, execRequestContext),
          new Dictionary<string, string>(),
          ciceeExecLinuxPath
        );
      }
    );
  }

  public static async Task<Result<ExecResult>> HandleAsync(CommandDependencies dependencies, ExecRequest request)
  {
    dependencies.StandardOutWriteLine(obj: "Beginning exec...\n");
    dependencies.StandardOutWriteLine($"Project root: {request.ProjectRoot}");
    dependencies.StandardOutWriteLine($"Entrypoint  : {request.Entrypoint}");
    dependencies.StandardOutWriteLine($"Command     : {request.Command}");

    return (await TryCreateRequestContext(dependencies, request)
      .Bind(execContext => ValidateContext(dependencies, execContext))
      .Map(context => DisplayExecContext(dependencies, context))
      .BindAsync(execContext => TryExecute(dependencies, execContext))).Map(context => new ExecResult(request));
  }

  private static string CreateCiDockerfilePath(CommandDependencies dependencies, ExecRequest request)
  {
    return dependencies.CombinePath(request.ProjectRoot, dependencies.CombinePath(CiDirectoryName, arg2: "Dockerfile"));
  }

  public static Result<ExecRequestContext> TryCreateRequestContext(CommandDependencies dependencies,
    ExecRequest request)
  {
    return dependencies.EnsureDirectoryExists(request.ProjectRoot).Bind(
      validatedProjectRoot => ProjectMetadataLoader.TryFindProjectMetadata(
        dependencies.EnsureDirectoryExists,
        dependencies.EnsureFileExists,
        dependencies.TryLoadFileString,
        dependencies.CombinePath,
        validatedProjectRoot
      ).Match(
        value => new Result<ProjectMetadata>(value.ProjectMetadata),
        loadFailure =>
          // We failed to load metadata, but we know that the project root exists.
          ProjectMetadataLoader.InferProjectMetadata(dependencies, validatedProjectRoot)
      )
    ).Bind(
      projectMetadata => Require.AsResult.NotNullOrWhitespace(request.Image)
        .BindFailure(_ => dependencies.EnsureFileExists(CreateCiDockerfilePath(dependencies, request))).MapFailure(
          exception => new BadRequestException(
            $"Image argument was not provided and '{CreateCiDockerfilePath(dependencies, request)}; does not exist.",
            exception
          )
        ).Map(_ => projectMetadata)
    ).Map(
      projectMetadata =>
      {
        string? dockerfile = dependencies.EnsureFileExists(
          dependencies.CombinePath(request.ProjectRoot, dependencies.CombinePath(CiDirectoryName, arg2: "Dockerfile"))
        ).Match(file => (string?)file, _ => null);
        return new ExecRequestContext(
          request.ProjectRoot,
          projectMetadata,
          request.Command,
          request.Entrypoint,
          dockerfile,
          request.Image
        );
      }
    );
  }

  private static ExecRequestContext DisplayExecContext(CommandDependencies dependencies,
    ExecRequestContext execRequestContext)
  {
    DisplayProjectEnvironmentValues();
    DisplayExecEnvironmentValues();

    return execRequestContext;

    void WriteEnvironmentVariables(IReadOnlyDictionary<string, string> environmentDisplay)
    {
      int width = environmentDisplay.Keys.Max(value => value.Length) + 1;
      foreach ((string key, string value) in environmentDisplay.OrderBy(kvp => kvp.Key))
      {
        dependencies.StandardOutWriteLine($"  {key.PadRight(width, paddingChar: ' ')}: {value}");
      }
    }

    void DisplayProjectEnvironmentValues()
    {
      ProjectEnvironmentHelpers.DisplayProjectEnvironmentValues(
        dependencies.StandardOutWriteLine,
        dependencies.StandardOutWrite,
        ProjectEnvironmentHelpers.GetEnvironmentDisplay(
          dependencies.GetEnvironmentVariables,
          execRequestContext.ProjectMetadata
        )
      );
    }

    void DisplayExecEnvironmentValues()
    {
      IReadOnlyDictionary<string, string> environmentDisplay = GetExecEnvironment(dependencies, execRequestContext);
      dependencies.StandardOutWriteLine(obj: "CICEE Execution Environment:");
      WriteEnvironmentVariables(environmentDisplay);
    }
  }

  private static IReadOnlyDictionary<string, string> GetExecEnvironment(CommandDependencies dependencies,
    ExecRequestContext context)
  {
    Dictionary<string, string> environment = new()
    {
      [CiExecContext] = Io.NormalizeToLinuxPath(dependencies.CombinePath(context.ProjectRoot, CiDirectoryName)),
      [ProjectName] = context.ProjectMetadata.Name,
      [ProjectRoot] = Io.NormalizeToLinuxPath(context.ProjectRoot),
      [LibRoot] = Io.NormalizeToLinuxPath(dependencies.GetLibraryRootPath())
    };

    ConditionallyAdd(CiCommand, context.Command);
    ConditionallyAdd(CiEntrypoint, context.Entrypoint);
    ConditionallyAdd(CiExecImage, context.Image);

    return environment;

    void ConditionallyAdd(string key, string? possibleValue)
    {
      if (!string.IsNullOrWhiteSpace(possibleValue))
      {
        environment[key] = possibleValue!;
      }
    }
  }


  private static async Task<Result<ExecRequestContext>> TryExecute(CommandDependencies dependencies,
    ExecRequestContext execRequestContext)
  {
    return (await CreateProcessStartInfo(dependencies, execRequestContext).BindAsync(dependencies.ProcessExecutor)).Map(
      _ => execRequestContext
    );
  }

  private static Result<ExecRequestContext> ValidateContext(CommandDependencies dependencies,
    ExecRequestContext execRequestContext)
  {
    return RequireStartupCommand(execRequestContext).Bind(RequireProjectRoot);

    static Result<ExecRequestContext> RequireStartupCommand(ExecRequestContext context)
    {
      // Require either a command
      return Require.AsResult.NotNullOrWhitespace(context.Command).BindFailure(
        missingCommandException =>
          // ... or an entrypoint
          Require.AsResult.NotNullOrWhitespace(context.Entrypoint)
      ).MapFailure(
        exception => new BadRequestException(
          message: "At least one of command or entrypoint must be provided.",
          exception
        )
      ).Map(_ => context);
    }

    Result<ExecRequestContext> RequireProjectRoot(ExecRequestContext contextWithStartupCommand)
    {
      return dependencies.EnsureDirectoryExists(contextWithStartupCommand.ProjectRoot).MapFailure(
        exception => exception is DirectoryNotFoundException
          ? new BadRequestException($"Project root '{contextWithStartupCommand.ProjectRoot}' cannot be found.")
          : exception
      ).Map(projectRoot => contextWithStartupCommand);
    }
  }
}
