using System;
using System.CommandLine;

using Cicee.Dependencies;

namespace Cicee.Commands.Env.Require;

public static class EnvRequireCommand
{
  public static Command Create(CommandDependencies dependencies)
  {
    Option<string?> projectRoot = ProjectRootOption.CreateOptional(dependencies);
    Option<string?> file = ProjectMetadataFile(
      () =>
      {
        LanguageExt.Option<string> possibleValue = ProjectMetadataOption.GetDefaultMetadataPathProvider(dependencies);
        return possibleValue.IsSome ? possibleValue.IfNone(string.Empty) : null;
      }
    );
    Command command = new(name: "require", description: "Require that the environment contains all required variables.")
    {
      projectRoot, file
    };
    command.SetHandler(
      (root, filePath) => EnvRequireEntrypoint.HandleAsync(dependencies, root, filePath),
      projectRoot,
      file
    );
    return command;
  }

  private static Option<string?> ProjectMetadataFile(Func<string?> getDefaultValue)
  {
    return new Option<string?>(
      new[]
      {
        "--metadata",
        "-m",
        "--file",
        "-f"
      },
      getDefaultValue,
      description: "Project metadata file."
    )
    {
      IsRequired = false
    };
  }
}
