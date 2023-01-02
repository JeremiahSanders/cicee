using System;
using System.CommandLine;
using Cicee.Dependencies;

namespace Cicee.Commands.Env.Require;

public static class EnvRequireCommand
{
  public static Command Create(CommandDependencies dependencies)
  {
    var projectRoot = ProjectRootOption.CreateOptional(dependencies);
    var file = ProjectMetadataFile(() =>
    {
      var possibleValue = ProjectMetadataOption.GetDefaultMetadataPathProvider(dependencies);
      return possibleValue.IsSome ? possibleValue.IfNone(string.Empty) : null;
    });
    var command = new Command(
      "require",
      "Require that the environment contains all required variables.") { projectRoot, file };
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
      new[] { "--metadata", "-m", "--file", "-f" },
      getDefaultValue,
      "Project metadata file."
    ) { IsRequired = false };
  }
}
