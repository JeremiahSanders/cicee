using System;
using System.Threading.Tasks;
using Cicee.Dependencies;

namespace Cicee.Commands.Meta.CiEnv.Variables.Update;

public static class MetaCiEnvVarUpdateEntrypoint
{
  public static Func<string, string, string?, bool?, bool?, string?, Task<int>> CreateHandler(
    CommandDependencies dependencies)
  {
    async Task<int> Handle(string projectMetadataPath, string name, string? description, bool? required,
      bool? secret, string? defaultValue)
    {
      return (await MetaCiEnvVarUpdateHandling.MetaCiEnvVarUpdateRequest(dependencies, projectMetadataPath, name,
          currentVariable => currentVariable.With(description, required, secret, defaultValue)))
        .TapSuccess(variablesAdded =>
        {
          dependencies.StandardOutWriteLine($"Updated {name}");
        })
        .TapFailure(exception =>
        {
          dependencies.StandardErrorWriteLine(exception.ToExecutionFailureMessage());
        })
        .ToExitCode();
    }

    return Handle;
  }
}
