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

  private static Result<ProcessExecRequest> TryCreateProcessExecRequest(
    ICommandDependencies dependencies,
    LibExecRequest request)
  {
    string projectRoot = request.Shell == LibraryShellTemplate.Bash
      ? Io.NormalizeToLinuxPath(request.ProjectRoot)
      : request.ProjectRoot;
    string projectMetadata = request.Shell == LibraryShellTemplate.Bash
      ? Io.NormalizeToLinuxPath(request.MetadataPath)
      : request.MetadataPath;
    string? execDir = dependencies
      .TryGetCurrentDirectory()
      .IfFail(projectRoot);

    return request.Shell switch
    {
      LibraryShellTemplate.Bash => CreateBash(),
      _ => new Result<ProcessExecRequest>(new BadRequestException($"Unsupported shell: {request.Shell}"))
    };

    Dictionary<string, string> GetProcessEnvironment()
    {
      Dictionary<string, string> env = dependencies
        .GetEnvironmentVariables()
        .ToDictionary(
          kvp => kvp.Key,
          kvp => kvp.Value
        );
      env[ProjectRoot] = projectRoot;
      env[ProjectMetadata] = projectMetadata;
      env[ExecDir] = execDir;

      Dictionary<string, string> fallbacks = request
        .Metadata.CiEnvironment.Variables.Where(IsDefaultNeeded)
        .ToDictionary(variable => variable.Name, variable => variable.DefaultValue!);

      return env
        .Concat(fallbacks)
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

      bool IsDefaultNeeded(ProjectEnvironmentVariable variable)
      {
        // The variable must have a default and it must not already be defined.
        return !string.IsNullOrWhiteSpace(variable.DefaultValue) && !env.Keys.Any(
          key => key.Equals(variable.Name, StringComparison.InvariantCultureIgnoreCase)
        );
      }
    }

    Result<ProcessExecRequest> CreateBash()
    {
      const string execScript = "exec.sh";
      string scriptPath = Io.NormalizeToLinuxPath(
        dependencies.CombinePath(LibraryPaths.Bash(dependencies), execScript)
      );
      string shellExecPath = $"{scriptPath} {request.Command}";

      return ProcessHelpers.TryCreateBashProcessStartInfo(
        GetRequiredEnvironment(),
        GetProcessEnvironment(),
        shellExecPath
      );
    }

    Dictionary<string, string> GetRequiredEnvironment()
    {
      return new Dictionary<string, string>
      {
        {
          ProjectRoot, projectRoot
        },
        {
          ProjectMetadata, projectMetadata
        }
      };
    }
  }

  public static Task<Result<LibExecResponse>> TryHandleAsync(ICommandDependencies dependencies, LibExecRequest request)
  {
    return TryCreateProcessExecRequest(dependencies, request)
      .BindAsync(
        async processExecRequest => (await dependencies.ProcessExecutor(processExecRequest, debugMessage => dependencies.LogDebug(debugMessage, ConsoleColor.DarkGray))).Map(
          processExecResult => new LibExecResponse(processExecRequest, processExecResult)
          {
            Shell = request.Shell
          }
        )
      );
  }

  private static Result<LibExecRequest> TryCreateRequest(
    ICommandDependencies dependencies,
    LibraryShellTemplate template,
    string command,
    string projectRoot,
    string metadataPath)
  {
    return Require
      .AsResult.NotNullOrWhitespace(command)
      .Bind(
        validatedCommand => dependencies
          .EnsureDirectoryExists(projectRoot)
          .Bind(
            validatedRoot => ProjectMetadataLoader
              .TryLoadFromFile(
                dependencies.EnsureFileExists,
                dependencies.TryLoadFileString,
                metadataPath
              )
              .Map(
                metadata => new LibExecRequest
                {
                  Command = validatedCommand,
                  Metadata = metadata,
                  MetadataPath = metadataPath,
                  ProjectRoot = validatedRoot,
                  Shell = template
                }
              )
          )
      );
  }

  public static async Task<int> HandleAsync(
    ICommandDependencies dependencies,
    LibraryShellTemplate template,
    string command,
    string projectRoot,
    string metadataPath)
  {
    return (await TryCreateRequest(dependencies, template, command, projectRoot, metadataPath)
        .TapSuccess(
          request => { dependencies.StandardOutWriteLine($"Beginning {request.Shell} CI shell execution."); }
        )
        .BindAsync(request => TryHandleAsync(dependencies, request)))
      .TapSuccess(
        response => { dependencies.StandardOutWriteLine(text: "Execution succeeded."); }
      )
      .TapFailure(
        exception => { dependencies.StandardErrorWriteLine(exception.ToExecutionFailureMessage()); }
      )
      .ToExitCode();
  }
}
