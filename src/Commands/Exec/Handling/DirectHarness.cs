using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Cicee.Dependencies;

using LanguageExt;
using LanguageExt.Common;

namespace Cicee.Commands.Exec.Handling;

public static class DirectHarness
{
  public static Task<ExecRequestContext> InvokeDockerCommandsAsync(
    CommandDependencies dependencies,
    ExecRequestContext execRequestContext
  )
  {
    return Prelude
      .Try(
        () =>
        {
          dependencies.LogDebug(message: "Preparing to Arrange.");

          return Prelude.unit;
        }
      )
      .ToAsync()
      .MapAsync(async _ => await Arrange(dependencies, execRequestContext))
      .TapSuccess(res => dependencies.LogDebug(message: "Arrangement complete. Preparing to Act."))
      .MapAsync(updatedContext => Act(dependencies, updatedContext))
      .TapSuccess(res => dependencies.LogDebug(message: "Act complete. Preparing to Cleanup."))
      .MapAsync(updatedContext => Cleanup(dependencies, updatedContext))
      .TapSuccess(res => dependencies.LogDebug(message: "Cleanup complete."))
      .IfFail(exception => Cleanup(dependencies, execRequestContext, exception));
  }

  private static Task<ExecRequestContext> Arrange(
    CommandDependencies dependencies,
    ExecRequestContext execRequestContext
  )
  {
    return Prelude
      .TryAsync(ConditionallyBuildProjectCi)
      .MapAsync(_ => PullDependencies())
      .IfFailThrow();

    async Task<ExecRequestContext> ConditionallyBuildProjectCi()
    {
      bool skipBuild = execRequestContext.Dockerfile == null || execRequestContext.CiDirectory == null;

      return skipBuild
        ? execRequestContext
        : await ExecuteCommandRequiringSuccess(
          dependencies,
          execRequestContext,
          (commandDependencies, context) => DockerBuild(
            commandDependencies,
            context,
            execRequestContext.Dockerfile!,
            execRequestContext.CiDirectory!
          )
        );
    }

    Task<ExecRequestContext> PullDependencies()
    {
      return ExecuteCommandRequiringSuccess(dependencies, execRequestContext, DockerComposePull);
    }
  }

  private static async Task<ExecRequestContext> Act(
    CommandDependencies dependencies,
    ExecRequestContext execRequestContext)
  {
    return await ExecuteCommandRequiringSuccess(
      dependencies,
      execRequestContext,
      DockerComposeUp
    );
  }

  private static async Task<ExecRequestContext> Cleanup(
    CommandDependencies dependencies,
    ExecRequestContext execRequestContext,
    Exception? exception = null
  )
  {
    ExecRequestContext result = await ExecuteCommandRequiringSuccess(
      dependencies,
      execRequestContext,
      DockerComposeDown
    );

    return exception != null ? Prelude.raise<ExecRequestContext>(exception) : result;
  }

  // private static IEnumerable<KeyValuePair<string,string>> GetContextEnvironmentVariables(
  //   this ExecRequestContext execRequestContext
  // )
  // {
  //   yield return new KeyValuePair<string, string>(HandlingConstants.CiExecContext, execRequestContext.CiDirectory ?? string.Empty);
  //   yield return new KeyValuePair<string, string>(HandlingConstants.ProjectName, execRequestContext.ProjectMetadata.Name);
  //   yield return new KeyValuePair<string, string>(HandlingConstants.ProjectRoot, execRequestContext.ProjectRoot);
  //   yield return new KeyValuePair<string, string>(HandlingConstants.LibRoot, execRequestContext.LibRoot ?? string.Empty);
  //
  //   yield return new KeyValuePair<string, string>(HandlingConstants.CiEntrypoint, execRequestContext.Entrypoint ?? string.Empty);
  //   yield return new KeyValuePair<string, string>(HandlingConstants.CiCommand, execRequestContext.Command ?? string.Empty);
  // }
  //
  // private static ProcessStartInfo SetEnvironmentFromContext(
  //   this ProcessStartInfo command,
  //   ExecRequestContext execRequestContext
  // )
  // {
  //   command.Environment[HandlingConstants.ProjectRoot] = execRequestContext.ProjectRoot;
  //   command.Environment[HandlingConstants.ProjectName] = execRequestContext.ProjectMetadata.Name;
  //
  //   command.Environment[HandlingConstants.LibRoot] = execRequestContext.LibRoot ?? string.Empty;
  //   command.Environment[HandlingConstants.CiExecContext] = execRequestContext.CiDirectory ?? string.Empty;
  //   command.Environment[HandlingConstants.CiEntrypoint] = execRequestContext.Entrypoint ?? string.Empty;
  //   command.Environment[HandlingConstants.CiCommand] = execRequestContext.Command ?? string.Empty;
  //
  //   return command;
  // }

  private static ProcessStartInfo DockerCommand(
    ExecRequestContext context,
    CommandDependencies dependencies,
    string command)
  {
    // IReadOnlyDictionary<string, string> requiredEnvironment, IReadOnlyDictionary<string, string> ambientEnvironment,
    ProcessStartInfo info = new(HandlingConstants.DockerCommand, command)
    {
      WorkingDirectory = context.ProjectRoot,
      UseShellExecute = false,
      Environment =
      {
        [HandlingConstants.DockerComposeProjectName] = context.ProjectMetadata.Name
      }
    };
    // var contextEnvironment = context.GetContextEnvironmentVariables();      
    // .SetEnvironmentFromContext(context);

    IReadOnlyDictionary<string, string> environment = IoEnvironment.GetExecEnvironment(
      dependencies,
      context,
      forcePathsToLinux: false
    );
    foreach (KeyValuePair<string, string> keyValuePair in environment)
    {
      info.Environment[keyValuePair.Key] = keyValuePair.Value;
    }

    return info;
  }

  private static ProcessStartInfo DockerBuild(
    CommandDependencies dependencies,
    ExecRequestContext context,
    string verifiedCiFilePath,
    string verifiedCiDirectory
  )
  {
    /*
__build_project_ci() {
if [[ -f "${PROJECT_ROOT}/ci/Dockerfile" ]]; then
docker build \
  --pull \
  --rm \
  --file "${PROJECT_ROOT}/ci/Dockerfile" \
  "${PROJECT_ROOT}/ci"
fi
}
 */
    return DockerCommand(
      context,
      dependencies,
      $"build --pull --rm --file \"{verifiedCiFilePath}\" \"{verifiedCiDirectory}\""
    );
  }

  private static ProcessStartInfo DockerComposePull(
    CommandDependencies dependencies,
    ExecRequestContext execRequestContext
  )
  {
    /*
__pull_dependencies() {
  # Explicit empty string default applied to prevent Docker Compose from reporting that it is defaulting to empty strings.
  LIB_ROOT="${LIB_ROOT:-}" \
    CI_EXEC_CONTEXT="${CI_EXEC_CONTEXT:-}" \
    CI_ENTRYPOINT="${CI_ENTRYPOINT:-}" \
    CI_COMMAND="${CI_COMMAND:-}" \
    ${docker_compose_executable} \
    "${COMPOSE_FILE_ARGS[@]}" \
    pull \
    --ignore-pull-failures \
    --include-deps \
    ci-exec
}
     */
    string composeFiles = string.Join(
      separator: " ",
      execRequestContext.DockerComposeFiles.Select(file => $"--file {file}")
    );

    return DockerCommand(
      execRequestContext,
      dependencies,
      $"compose {composeFiles} pull --ignore-pull-failures --include-deps {HandlingConstants.DockerComposeServiceCiExec}"
    );
  }

  private static ProcessStartInfo DockerComposeUp(
    CommandDependencies dependencies,
    ExecRequestContext execRequestContext
  )
  {
    /*
  # Explicit empty string default applied to prevent Docker Compose from reporting that it is defaulting to empty strings.
  LIB_ROOT="${LIB_ROOT:-}" \
    CI_EXEC_CONTEXT="${CI_EXEC_CONTEXT:-}" \
    CI_ENTRYPOINT="${CI_ENTRYPOINT:-}" \
    CI_COMMAND="${CI_COMMAND:-}" \
    COMPOSE_PROJECT_NAME="${PROJECT_NAME}" \
    ${docker_compose_executable} \
    "${COMPOSE_FILE_ARGS[@]}" \
    up \
    --abort-on-container-exit \
    --build \
    --renew-anon-volumes \
    --remove-orphans \
    ci-exec
     */
    string composeFiles = string.Join(
      separator: " ",
      execRequestContext.DockerComposeFiles.Select(file => $"--file {file}")
    );

    return DockerCommand(
      execRequestContext,
      dependencies,
      $"compose {composeFiles} up --abort-on-container-exit --build --renew-anon-volumes --remove-orphans {HandlingConstants.DockerComposeServiceCiExec}"
    );
  }

  private static ProcessStartInfo DockerComposeDown(
    CommandDependencies dependencies,
    ExecRequestContext execRequestContext
  )
  {
    /*
  # Explicit empty string default applied to prevent Docker Compose from reporting that it is defaulting to empty strings.
  LIB_ROOT="${LIB_ROOT:-}" \
    CI_EXEC_CONTEXT="${CI_EXEC_CONTEXT:-}" \
    CI_ENTRYPOINT="${CI_ENTRYPOINT:-}" \
    CI_COMMAND="${CI_COMMAND:-}" \
    COMPOSE_PROJECT_NAME="${PROJECT_NAME}" \
    ${docker_compose_executable} \
    "${COMPOSE_FILE_ARGS[@]}" \
    down \
    --volumes \
    --remove-orphans
     */
    string composeFiles = string.Join(
      separator: " ",
      execRequestContext.DockerComposeFiles.Select(file => $"--file {file}")
    );

    return DockerCommand(
      execRequestContext,
      dependencies,
      $"compose {composeFiles} down --volumes --remove-orphans"
    );
  }

  private static async Task<ExecRequestContext> ExecuteCommandRequiringSuccess(
    CommandDependencies dependencies,
    ExecRequestContext execRequestContext,
    Func<CommandDependencies, ExecRequestContext, ProcessStartInfo> creatorFunc
  )
  {
    ProcessStartInfo processStartInfo = creatorFunc(dependencies, execRequestContext);
    dependencies.LogDebug($"Preparing to execute: {processStartInfo.FileName} {processStartInfo.Arguments}");
    // dependencies.LogDebug($"  Execution environment:{Environment.NewLine}{string.Join(Environment.NewLine, processStartInfo.Environment.Select(kvp=>$"    {kvp.Key}={kvp.Value}"))}");

    Result<ProcessExecResult> result = await dependencies.ProcessExecutor(processStartInfo);

    string? resultDisplay = result
      .Map(_ => "successfully")
      .IfFail(
        exception => exception is ExecutionException executionException
          ? $"in failure: {executionException.Message}"
          : $"in failure: {exception}"
      );

    dependencies.LogDebug($"Completed execution {resultDisplay}.", result.IsFaulted ? ConsoleColor.Red : null);

    ProcessExecResult success = result.IfFailThrow();
    success.RequireExitCodeZero();

    return execRequestContext;
  }
}
