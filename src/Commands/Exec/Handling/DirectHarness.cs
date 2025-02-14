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
  
  /// <summary>
  ///   Executes <see cref="Cicee.Dependencies.CommandDependencies.LogDebug" /> when
  ///   <see cref="ExecRequestContext.WorkflowQuiet" /> is <c>false</c>.
  /// </summary>
  private static void MaybeLogDebug(
    this CommandDependencies dependencies,
    ExecRequestContext context,
    string message,
    ConsoleColor? color = null)
  {
    if (!context.WorkflowQuiet)
    {
      dependencies.LogDebug(message, color);
    }
  }

  public static Task<ExecRequestContext> InvokeDockerCommandsAsync(
    CommandDependencies dependencies,
    ExecRequestContext execRequestContext
  )
  {
    return Prelude
      .Try(
        () =>
        {
          dependencies.MaybeLogDebug(execRequestContext, message: "Preparing to Arrange.");

          return Prelude.unit;
        }
      )
      .ToAsync()
      .MapAsync(async _ => await Arrange(dependencies, execRequestContext))
      .TapSuccess(
        res => dependencies.MaybeLogDebug(execRequestContext, message: "Arrangement complete. Preparing to Act.")
      )
      .MapAsync(updatedContext => Act(dependencies, updatedContext))
      .TapSuccess(
        res => dependencies.MaybeLogDebug(execRequestContext, message: "Act complete. Preparing to Cleanup.")
      )
      .MapAsync(updatedContext => Cleanup(dependencies, updatedContext))
      .TapSuccess(res => dependencies.MaybeLogDebug(execRequestContext, message: "Cleanup complete."))
      .IfFail(exception => Cleanup(dependencies, execRequestContext, exception));
  }

  private static Task<ExecRequestContext> Arrange(
    CommandDependencies dependencies,
    ExecRequestContext execRequestContext
  )
  {
    return Prelude
      // TODO: Reconsider local Dockerfile build and cache. Might be useful for performance.
      // .TryAsync(ConditionallyBuildProjectCi)
      // .MapAsync(_ => PullDependencies())
      .TryAsync(PullDependencies)
      .IfFailThrow();

    async Task<ExecRequestContext> ConditionallyBuildProjectCi()
    {
      // This method is currently unused.
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
            execRequestContext.CiDirectory!,
            execRequestContext.CiDockerfileImageTag
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
    ExecRequestContext composeDownResult = await ExecuteCommandRequiringSuccess(
      dependencies,
      execRequestContext,
      DockerComposeDown
    );
    // ExecRequestContext ciImageRemoveResult = await ExecuteCommandRequiringSuccess(
    //   dependencies,
    //   composeDownResult,
    //   DockerCiImageRemove
    // );

    return exception != null ? Prelude.raise<ExecRequestContext>(exception) : composeDownResult;
  }

  private static ProcessStartInfo DockerCommand(
    ExecRequestContext context,
    CommandDependencies dependencies,
    string command)
  {
    ProcessStartInfo info = new(HandlingConstants.DockerCommand, command)
    {
      WorkingDirectory = context.ProjectRoot,
      UseShellExecute = false,
      Environment =
      {
        [HandlingConstants.DockerComposeProjectName] = context.ProjectMetadata.Name
      }
    };

    ApplyExecEnvironment(info);

    return info;

    ProcessStartInfo ApplyExecEnvironment(ProcessStartInfo initialInfo)
    {
      IReadOnlyDictionary<string, string> execEnvironment = IoEnvironment.GetExecEnvironment(
        dependencies,
        context,
        forcePathsToLinux: false
      );
      foreach (KeyValuePair<string, string> keyValuePair in execEnvironment)
      {
        initialInfo.Environment[keyValuePair.Key] = keyValuePair.Value;
      }

      return initialInfo;
    }
  }

  private static ProcessStartInfo DockerBuild(
    CommandDependencies dependencies,
    ExecRequestContext context,
    string verifiedCiFilePath,
    string verifiedCiDirectory,
    string imageTag
  )
  {
    string quiet = context.DockerQuiet
      ? "--quiet "
      : string.Empty;

    return DockerCommand(
      context,
      dependencies,
      $"build {quiet}--pull --tag \"{imageTag}\" --file \"{verifiedCiFilePath}\" \"{verifiedCiDirectory}\""
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
    string quiet = execRequestContext.DockerQuiet
      ? "--quiet "
      : string.Empty;

    return DockerCommand(
      execRequestContext,
      dependencies,
      $"compose {composeFiles} pull {quiet}--ignore-pull-failures --include-deps {HandlingConstants.DockerComposeServiceCiExec}"
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

  private static ProcessStartInfo DockerCiImageRemove(
    CommandDependencies dependencies,
    ExecRequestContext execRequestContext
  )
  {
    return DockerCommand(
      execRequestContext,
      dependencies,
      $"image rm {execRequestContext.CiDockerfileImageTag}"
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
    dependencies.MaybeLogDebug(
      execRequestContext,
      $"Preparing to execute: {processStartInfo.FileName} {processStartInfo.Arguments}"
    );
    // dependencies.LogDebug($"  Execution environment:{Environment.NewLine}{string.Join(Environment.NewLine, processStartInfo.Environment.Select(kvp=>$"    {kvp.Key}={kvp.Value}"))}");

    Result<ProcessExecResult> result = await dependencies.ProcessExecutor(processStartInfo);

    string? resultDisplay = result
      .Map(_ => "successfully")
      .IfFail(
        exception => exception is ExecutionException executionException
          ? $"in failure: {executionException.Message}"
          : $"in failure: {exception}"
      );

    dependencies.MaybeLogDebug(
      execRequestContext,
      $"Completed execution {resultDisplay}.",
      result.IsFaulted ? ConsoleColor.Red : null
    );

    ProcessExecResult success = result.IfFailThrow();
    success.RequireExitCodeZero();

    return execRequestContext;
  }
}
