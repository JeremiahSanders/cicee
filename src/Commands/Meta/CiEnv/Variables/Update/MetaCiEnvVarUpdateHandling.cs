using System;
using System.Linq;
using System.Threading.Tasks;
using Cicee.CiEnv;
using Cicee.Commands.Exec;
using Cicee.Dependencies;
using LanguageExt.Common;

namespace Cicee.Commands.Meta.CiEnv.Variables.Update;

public static class MetaCiEnvVarUpdateHandling
{
  internal static ProjectEnvironmentVariable With(this ProjectEnvironmentVariable currentVariable,
    string? description,
    bool? required,
    bool? secret,
    string? defaultValue
  )
  {
    return currentVariable with
    {
      Description = description ?? currentVariable.Description,
      DefaultValue = defaultValue ?? currentVariable.DefaultValue,
      Required = required ?? currentVariable.Required,
      Secret = secret ?? currentVariable.Secret
    };
  }

  public static async Task<Result<ProjectEnvironmentVariable[]>> MetaCiEnvVarUpdateRequest(
    CommandDependencies dependencies,
    string projectMetadataPath,
    string name,
    Func<ProjectEnvironmentVariable, ProjectEnvironmentVariable> mutator,
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
            AllVariables: Enumerable.Empty<ProjectEnvironmentVariable>()),
          (enumerables, variable) =>
          {
            var isMatch = variable.Name.ToUpperInvariant().Equals(comparisonName);
            var toAdd = isMatch ? mutator(variable) : variable;
            return isMatch
              ? (enumerables.Matches.Append(toAdd), enumerables.AllVariables.Append(toAdd))
              : (enumerables.Matches, enumerables.AllVariables.Append(variable));
          })
      )
      .Map(enumerables =>
        new { Matches = enumerables.Matches.ToArray(), AllVariables = enumerables.AllVariables.ToArray() })
      .Filter(variables => variables.Matches.Any(),
        _ => new ExecutionException($"Variable \"{name}\" does not exist.", exitCode: 1)
      )
      .BindAsync(async variables =>
      {
        return isDryRun
          ? new Result<ProjectEnvironmentVariable[]>(variables.Matches)
          : (await ProjectMetadataManipulation.UpdateVariablesInMetadata(dependencies, projectMetadataPath,
            variables.AllVariables)).Map(_ => variables.Matches);
      });
  }
}
