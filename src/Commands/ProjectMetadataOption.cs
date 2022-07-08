using System;
using Cicee.CiEnv;
using Cicee.Dependencies;
using LanguageExt;

namespace Cicee.Commands;

public static class ProjectMetadataOption
{
  public static System.CommandLine.Option<string> Of(Func<string> getDefaultValue, bool isRequired = true)
  {
    return new System.CommandLine.Option<string>(
      new[] {"--metadata", "-m"}, getDefaultValue,
      "Project metadata file path."
    ) {IsRequired = isRequired};
  }

  public static LanguageExt.Option<string> GetDefaultMetadataPathProvider(CommandDependencies dependencies)
  {
    LanguageExt.Option<string> GetDefaultFileName()
    {
      return dependencies.TryGetCurrentDirectory()
        .Map(currentDirectory =>
          ProjectMetadataLoader.CreateDefaultMetadataFileName(dependencies.CombinePath, currentDirectory)
        )
        .Map(Prelude.Some)
        .IfFail(Prelude.None);
    }

    return
      ProjectMetadataLoader.InferProjectMetadataPath(
          dependencies.EnsureDirectoryExists,
          dependencies.EnsureFileExists,
          dependencies.TryLoadFileString,
          dependencies.CombinePath,
          dependencies.TryGetParentDirectory,
          dependencies.TryGetCurrentDirectory
        )
        .Match(Prelude.Some, _ => Prelude.None)
        .BiBind(Prelude.Some, GetDefaultFileName);
  }

  public static System.CommandLine.Option<string> Create(CommandDependencies dependencies, bool isRequired = true)
  {
    return Of(() => GetDefaultMetadataPathProvider(dependencies).IfNone(string.Empty), isRequired);
  }
}
