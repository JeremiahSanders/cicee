using System;
using System.Threading.Tasks;

using Cicee.CiEnv;
using Cicee.Dependencies;

namespace Cicee.Commands.Meta.CiEnv.Variables.Add;

public static class MetaCiEnvVarAddEntrypoint
{
  public static Func<string, string, string, bool, bool, string?, Task<int>> CreateHandler(
    CommandDependencies dependencies)
  {
    return Handle;

    async Task<int> Handle(string projectMetadataPath, string name, string description, bool required, bool secret,
      string? defaultValue)
    {
      ProjectEnvironmentVariable variable = new()
      {
        DefaultValue = defaultValue,
        Description = description,
        Name = name,
        Required = required,
        Secret = secret
      };
      return (await MetaCiEnvVarAddHandling.MetaCiEnvVarAddRequest(dependencies, projectMetadataPath, variable))
        .TapSuccess(
          variablesAdded =>
          {
            dependencies.StandardOutWriteLine($"Added {variable.Name}");
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
