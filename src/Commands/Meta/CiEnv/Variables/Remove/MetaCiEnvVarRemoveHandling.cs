using System.Linq;
using System.Threading.Tasks;
using Cicee.CiEnv;
using Cicee.Commands.Exec;
using Cicee.Dependencies;
using LanguageExt.Common;

namespace Cicee.Commands.Meta.CiEnv.Variables.Remove;

public static class MetaCiEnvVarRemoveHandling
{
  public static async Task<Result<ProjectEnvironmentVariable[]>> MetaCiEnvVarRemoveRequest(
    CommandDependencies dependencies,
    string projectMetadataPath,
    string name,
    bool isDryRun = false
  )
  {
    var comparisonName = name.ToUpperInvariant();
    return await ProjectMetadataLoader.TryLoadFromFile(
        dependencies.EnsureFileExists,
        dependencies.TryLoadFileString,
        projectMetadataPath
      )
      .Map(metadata =>
        metadata.CiEnvironment.Variables.Fold(
          (Matches: Enumerable.Empty<ProjectEnvironmentVariable>(),
            NonMatches: Enumerable.Empty<ProjectEnvironmentVariable>()),
          (enumerables, variable) =>
            variable.Name.ToUpperInvariant().Equals(comparisonName)
              ? (enumerables.Matches.Append(variable), enumerables.NonMatches)
              : (enumerables.Matches, enumerables.NonMatches.Append(variable))
        )
      )
      .Map(enumerables =>
        new { Matches = enumerables.Matches.ToArray(), NonMatches = enumerables.NonMatches.ToArray() })
      .Filter(variables => variables.Matches.Any(),
        _ => new ExecutionException($"Variable \"{name}\" does not exist.", exitCode: 1)
      )
      .BindAsync(async variables =>
      {
        return isDryRun
          ? new Result<ProjectEnvironmentVariable[]>(variables.Matches)
          : (await ProjectMetadataManipulation.UpdateVariablesInMetadata(dependencies, projectMetadataPath,
            variables.NonMatches)).Map(_ => variables.Matches);
      });
  }
}
