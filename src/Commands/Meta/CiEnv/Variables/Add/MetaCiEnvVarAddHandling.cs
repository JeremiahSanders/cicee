using System.Linq;
using System.Threading.Tasks;

using Cicee.CiEnv;
using Cicee.Commands.Exec;
using Cicee.Dependencies;

using LanguageExt.Common;

namespace Cicee.Commands.Meta.CiEnv.Variables.Add;

public static class MetaCiEnvVarAddHandling
{
  private static bool DoesNotContainVariable(ProjectMetadata metadata, ProjectEnvironmentVariable variable)
  {
    return !metadata.CiEnvironment.Variables.Select(ciVariable => ciVariable.Name.ToUpperInvariant())
      .Contains(variable.Name.ToUpperInvariant());
  }

  public static async Task<Result<ProjectEnvironmentVariable[]>> MetaCiEnvVarAddRequest(
    CommandDependencies dependencies, string projectMetadataPath, ProjectEnvironmentVariable variable,
    bool isDryRun = false)
  {
    return await ProjectMetadataLoader
      .TryLoadFromFile(dependencies.EnsureFileExists, dependencies.TryLoadFileString, projectMetadataPath).Filter(
        metadata => DoesNotContainVariable(metadata, variable),
        _ => new ExecutionException($"Variable \"{variable.Name}\" already exists.", exitCode: 1)
      ).BindAsync(
        async metadata =>
        {
          ProjectEnvironmentVariable[] newVariables =
          {
            variable
          };
          ProjectEnvironmentVariable[] updatedVariables =
            metadata.CiEnvironment.Variables.Append(newVariables).ToArray();
          return isDryRun
            ? new Result<ProjectEnvironmentVariable[]>(updatedVariables)
            : await ProjectMetadataManipulation.UpdateVariablesInMetadata(
              dependencies,
              projectMetadataPath,
              updatedVariables
            );
        }
      );
  }
}
