using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Cicee.Dependencies;

using LanguageExt;
using LanguageExt.Common;

namespace Cicee.Commands.Exec.Handling;

public static class DirectHarness
{
  public static Task<ExecRequestContext> InvokeDockerCommandsAsync(CommandDependencies dependencies,
    ExecRequestContext execRequestContext)
  {
    return Prelude
      .TryAsync(() => Arrange(dependencies, execRequestContext))
      .MapAsync(updatedContext => Act(dependencies, updatedContext))
      .MapAsync(updatedContext => Cleanup(dependencies, updatedContext))
      .IfFail(exception => Cleanup(dependencies, execRequestContext, exception));
  }

  private static Task<ExecRequestContext> Arrange(CommandDependencies dependencies,
    ExecRequestContext execRequestContext)
  {
    return Prelude
      .TryAsync(ConditionallyBuildProjectCi)
      .MapAsync(_ => PullDependencies())
      .IfFailThrow();

    async Task<ExecRequestContext> ConditionallyBuildProjectCi()
    {
      return execRequestContext.Dockerfile == null || execRequestContext.CiDirectory == null
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

  private static async Task<ExecRequestContext> Act(CommandDependencies dependencies,
    ExecRequestContext execRequestContext)
  {
    return await ExecuteCommandRequiringSuccess(dependencies, execRequestContext, DockerComposeUp);
  }

  private static async Task<ExecRequestContext> Cleanup(CommandDependencies dependencies,
    ExecRequestContext execRequestContext, Exception? exception = null)
  {
    ExecRequestContext result = await ExecuteCommandRequiringSuccess(
      dependencies,
      execRequestContext,
      DockerComposeDown
    );

    return exception != null ? Prelude.raise<ExecRequestContext>(exception) : result;
  }

  private static ProcessStartInfo SetEnvironmentFromContext(this ProcessStartInfo command,
    ExecRequestContext execRequestContext)
  {
    command.Environment[HandlingConstants.LibRoot] = execRequestContext.LibRoot ?? string.Empty;
    command.Environment[HandlingConstants.CiExecContext] = execRequestContext.CiDirectory ?? string.Empty;
    command.Environment[HandlingConstants.CiEntrypoint] = execRequestContext.Entrypoint ?? string.Empty;
    command.Environment[HandlingConstants.CiCommand] = execRequestContext.Command ?? string.Empty;

    return command;
  }

  private static ProcessStartInfo DockerBuild(CommandDependencies commandDependencies, ExecRequestContext context,
    string verifiedCiFilePath, string verifiedCiDirectory)
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
    return new ProcessStartInfo(
      HandlingConstants.DockerCommand,
      $"build --pull --rm --file \"{verifiedCiFilePath}\" \"{verifiedCiDirectory}\""
    );
  }

  private static ProcessStartInfo DockerComposePull(CommandDependencies dependencies,
    ExecRequestContext execRequestContext)
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
    string composeFiles = string.Join(separator: " ", execRequestContext.DockerComposeFiles);

    return new ProcessStartInfo(
      HandlingConstants.DockerCommand,
      $"compose {composeFiles} pull --ignore-pull-failures --include-deps {HandlingConstants.DockerComposeServiceCiExec}"
    ).SetEnvironmentFromContext(execRequestContext);
  }

  private static ProcessStartInfo DockerComposeUp(CommandDependencies dependencies,
    ExecRequestContext execRequestContext)
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
    string composeFiles = string.Join(separator: " ", execRequestContext.DockerComposeFiles);

    return new ProcessStartInfo(
      HandlingConstants.DockerCommand,
      $"compose {composeFiles} up --abort-on-container-exit --build --renew-anon-volumes --remove-orphans {HandlingConstants.DockerComposeServiceCiExec}"
    ).SetEnvironmentFromContext(execRequestContext);
  }

  private static ProcessStartInfo DockerComposeDown(CommandDependencies dependencies,
    ExecRequestContext execRequestContext)
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
    string composeFiles = string.Join(separator: " ", execRequestContext.DockerComposeFiles);

    return new ProcessStartInfo(
      HandlingConstants.DockerCommand,
      $"compose {composeFiles} down --volumes --remove-orphans"
    ).SetEnvironmentFromContext(execRequestContext);
  }

  private static async Task<ExecRequestContext> ExecuteCommandRequiringSuccess(CommandDependencies dependencies,
    ExecRequestContext execRequestContext, Func<CommandDependencies, ExecRequestContext, ProcessStartInfo> creatorFunc)
  {
    ProcessStartInfo dockerComposeDownRequest = creatorFunc(dependencies, execRequestContext);
    Result<ProcessExecResult> result = await dependencies.ProcessExecutor(dockerComposeDownRequest);
    ProcessExecResult success = result.IfFailThrow();
    success.RequireExitCodeZero();

    return execRequestContext;
  }
}
