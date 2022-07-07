using System;
using System.CommandLine;
using Cicee.Dependencies;

namespace Cicee.Commands.Env.Require;

public static class EnvRequireCommand
{
  public static Command Create(CommandDependencies dependencies)
  {
    var projectRoot = OptionalProjectRootOption(dependencies);
    var file = ProjectMetadataFile(() =>
    {
      var possibleValue = ProjectMetadataOption.GetDefaultMetadataPathProvider(dependencies);
      return possibleValue.IsSome ? possibleValue.IfNone(string.Empty) : null;
    });
    var command = new Command(
      "require",
      "Require that the environment contains all required variables.") {projectRoot, file};
    command.SetHandler<string?, string?>(EnvRequireEntrypoint.HandleAsync, projectRoot, file);
    return command;
  }

  private static Option OptionalProjectRootOption(CommandDependencies dependencies)
  {
    var option = ProjectRootOption.Create(dependencies);
    option.IsRequired = false;
    return option;
  }

  private static Option<string?> ProjectMetadataFile(Func<string?> getDefaultValue)
  {
    return new Option<string?>(
      new[] {"--metadata", "-m", "--file", "-f"},
      getDefaultValue,
      "Project metadata file."
    ) {IsRequired = false};
  }
}
