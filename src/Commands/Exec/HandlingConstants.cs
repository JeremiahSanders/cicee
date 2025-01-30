using Cicee.CiEnv;

namespace Cicee.Commands.Exec;

internal static class HandlingConstants
{
  public const string CiDirectoryName = Conventions.CiDirectoryName;
  public const string ProjectName = "PROJECT_NAME";
  public const string ProjectRoot = "PROJECT_ROOT";
  public const string LibRoot = "LIB_ROOT";
  public const string CiCommand = "CI_COMMAND";
  public const string CiEntrypoint = "CI_ENTRYPOINT";
  public const string CiExecImage = "CI_EXEC_IMAGE";

  public const string DockerComposeServiceCiExec = "ci-exec";

  /// <summary>
  ///   Build context for ci-exec service.
  /// </summary>
  public const string CiExecContext = "CI_EXEC_CONTEXT";

  public const string CiceeExecScriptName = "cicee-exec.sh";

  public const string DockerCommand = "docker";
  public const string DockerComposeArgument = "compose";
  public const string DockerComposeProjectName = "COMPOSE_PROJECT_NAME";
}
