using System;
using System.Linq;
using System.Threading.Tasks;

using Cicee.Dependencies;

namespace Cicee.Commands.Meta.CiEnv.Variables.Remove;

public static class MetaCiEnvVarRemoveEntrypoint
{
  public static Func<string, string, Task<int>> CreateHandler(CommandDependencies dependencies)
  {
    return Handle;

    async Task<int> Handle(string projectMetadataPath, string name)
    {
      return (await MetaCiEnvVarRemoveHandling.MetaCiEnvVarRemoveRequest(dependencies, projectMetadataPath, name))
        .TapSuccess(
          variablesRemoved =>
          {
            dependencies.StandardOutWriteAll(
              new (ConsoleColor?, string)[]
              {
                (ConsoleColor.Red, $"Removed:{Environment.NewLine}")
              }.Append(
                variablesRemoved.Select(variable => ((ConsoleColor?)null, $"  {variable.Name}{Environment.NewLine}"))
              )
            );
          }
        ).TapFailure(
          exception =>
          {
            dependencies.StandardErrorWriteLine(exception.ToExecutionFailureMessage());
          }
        ).ToExitCode();
    }
  }
}
