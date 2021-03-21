using Cicee.CiEnv;

namespace Cicee.Commands.Env.Require
{
  public record EnvRequireRequestContext(string FilePath, ProjectMetadata ProjectMetadata);
}
