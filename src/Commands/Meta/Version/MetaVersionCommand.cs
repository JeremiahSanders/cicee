using System;
using System.CommandLine;
using Cicee.CiEnv;
using LanguageExt;

namespace Cicee.Commands.Meta.Version;

public static class MetaVersionCommand
{
  public const string CommandName = "version";
  public const string CommandDescription = "Gets version in project metadata.";

  private static System.CommandLine.Option<string> CreateProjectMetadataOption(Func<string> getDefaultValue)
  {
    return new System.CommandLine.Option<string>(
      new[] {"--metadata", "-m"}, getDefaultValue,
      "Project metadata file path."
    ) {IsRequired = true};
  }

  public static Command Create(CommandDependencies dependencies)
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

    var projectMetadata = CreateProjectMetadataOption(() =>
      ProjectMetadataLoader.InferProjectMetadataPath(
          dependencies.EnsureDirectoryExists,
          dependencies.EnsureFileExists,
          dependencies.TryLoadFileString,
          dependencies.CombinePath,
          dependencies.TryGetParentDirectory,
          dependencies.TryGetCurrentDirectory
        )
        .Match(Prelude.Some, _ => Prelude.None)
        .BiBind(Prelude.Some, GetDefaultFileName)
        .IfNone(string.Empty)
    );
    var command = new Command(CommandName, CommandDescription) {projectMetadata};
    command.SetHandler(MetaVersionEntrypoint.CreateHandler(dependencies), projectMetadata);

    return command;
  }
}
