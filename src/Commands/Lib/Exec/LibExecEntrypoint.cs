using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cicee.CiEnv;
using Cicee.Dependencies;
using LanguageExt.Common;

namespace Cicee.Commands.Lib.Exec;

public static class LibExecEntrypoint
{
  private const string ProjectRoot = "PROJECT_ROOT";
  private const string ProjectMetadata = "PROJECT_METADATA";
  private const string ExecDir = "EXEC_DIR";

  private static Result<ProcessStartInfo> TryCreateProcessStartInfo(CommandDependencies dependencies,
    LibExecRequest request)
  {
    var projectRoot = request.Shell == LibraryShellTemplate.Bash
      ? Io.NormalizeToLinuxPath(request.ProjectRoot)
      : request.ProjectRoot;
    var projectMetadata = request.Shell == LibraryShellTemplate.Bash
      ? Io.NormalizeToLinuxPath(request.MetadataPath)
      : request.MetadataPath;
    var execDir = dependencies.TryGetCurrentDirectory().IfFail(projectRoot);

    return request.Shell switch
    {
      LibraryShellTemplate.Bash => CreateBash(),
      _ => new Result<ProcessStartInfo>(new BadRequestException($"Unsupported shell: {request.Shell}"))
    };

    Dictionary<string, string> GetProcessEnvironment()
    {
      var env = dependencies.GetEnvironmentVariables()
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
      env[ProjectRoot] = projectRoot;
      env[ProjectMetadata] = projectMetadata;
      env[ExecDir] = execDir;

      var fallbacks = request.Metadata.CiEnvironment.Variables
        .Where(IsDefaultNeeded)
        .ToDictionary(variable => variable.Name, variable => variable.DefaultValue!);

      return env.Concat(fallbacks).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

      bool IsDefaultNeeded(ProjectEnvironmentVariable variable)
      {
        // The variable must have a default and it must not already be defined.
        return !string.IsNullOrWhiteSpace(variable.DefaultValue) &&
               !env.Keys.Any(key => key.Equals(variable.Name, StringComparison.InvariantCultureIgnoreCase));
      }
    }

    Result<ProcessStartInfo> CreateBash()
    {
      const string execScript = "exec.sh";
      var scriptPath = Io.NormalizeToLinuxPath(
        dependencies.CombinePath(LibraryPaths.Bash(dependencies), execScript)
      );
      var shellExecPath = $"{scriptPath} {request.Command}";

      return ProcessHelpers.TryCreateBashProcessStartInfo(
        GetRequiredEnvironment(),
        GetProcessEnvironment(),
        shellExecPath
      );
    }

    Dictionary<string, string> GetRequiredEnvironment()
    {
      return new Dictionary<string, string> {{ProjectRoot, projectRoot}, {ProjectMetadata, projectMetadata}};
    }
  }

  public static Task<Result<LibExecResponse>> TryHandleAsync(CommandDependencies dependencies, LibExecRequest request)
  {
    return TryCreateProcessStartInfo(dependencies, request)
      .BindAsync(
        async processStartInfo =>
          (await dependencies.ProcessExecutor(processStartInfo))
          .Map(processExecResult =>
            new LibExecResponse(processStartInfo, processExecResult) {Shell = request.Shell}
          )
      );
  }

  private static Result<LibExecRequest> TryCreateRequest(
    CommandDependencies dependencies,
    LibraryShellTemplate template,
    string command,
    string projectRoot,
    string metadataPath
  )
  {
    return Require.AsResult.NotNullOrWhitespace(command)
      .Bind(validatedCommand =>
        dependencies.EnsureDirectoryExists(projectRoot)
          .Bind(
            validatedRoot => ProjectMetadataLoader.TryLoadFromFile(
                dependencies.EnsureFileExists,
                dependencies.TryLoadFileString,
                metadataPath
              )
              .Map(metadata => new LibExecRequest
              {
                Command = validatedCommand,
                Metadata = metadata,
                MetadataPath = metadataPath,
                ProjectRoot = validatedRoot,
                Shell = template
              })
          )
      );
  }

  public static async Task<int> HandleAsync(
    CommandDependencies dependencies,
    LibraryShellTemplate template,
    string command,
    string projectRoot,
    string metadataPath
  )
  {
    return (await TryCreateRequest(dependencies, template, command, projectRoot, metadataPath)
        .TapSuccess(request =>
        {
          dependencies.StandardOutWriteLine($"Beginning {request.Shell} CI shell execution.");
        })
        .BindAsync(request => TryHandleAsync(dependencies, request)))
      .TapSuccess(response =>
      {
        dependencies.StandardOutWriteLine("Execution succeeded.");
      })
      .TapFailure(exception =>
      {
        dependencies.StandardErrorWriteLine(exception.ToExecutionFailureMessage());
      })
      .ToExitCode();
  }
}
