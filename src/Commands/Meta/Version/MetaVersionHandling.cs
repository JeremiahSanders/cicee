using Cicee.CiEnv;
using LanguageExt.Common;

namespace Cicee.Commands.Meta.Version;

public static class MetaVersionHandling
{
  public static Result<string> HandleMetaVersionRequest(CommandDependencies dependencies, string projectMetadataPath)
  {
    return ProjectMetadataLoader.TryLoadFromFile(
        dependencies.EnsureFileExists,
        dependencies.TryLoadFileString,
        projectMetadataPath
      )
      .Map(metadata => metadata.Version);
  }
}
