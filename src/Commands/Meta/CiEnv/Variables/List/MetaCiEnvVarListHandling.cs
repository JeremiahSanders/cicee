using System;
using System.Linq;
using Cicee.CiEnv;
using Cicee.Dependencies;
using LanguageExt.Common;

namespace Cicee.Commands.Meta.CiEnv.Variables.List;

public static class MetaCiEnvVarListHandling
{
  public static Result<ProjectEnvironmentVariable[]> MetaCiEnvVarListRequest(
    CommandDependencies dependencies,
    string projectMetadataPath,
    string? nameContains
  )
  {
    return ProjectMetadataLoader.TryLoadFromFile(
        dependencies.EnsureFileExists,
        dependencies.TryLoadFileString,
        projectMetadataPath
      )
      .Map(metadata =>
        nameContains == null
          ? metadata.CiEnvironment.Variables
          : metadata.CiEnvironment.Variables
            .Where(variable => variable.Name.Contains(nameContains, StringComparison.InvariantCultureIgnoreCase))
            .ToArray()
      );
  }
}
