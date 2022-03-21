using System.CommandLine;

namespace Cicee.Commands.Env.Require;

public static class EnvRequireCommand
{
  public static Command Create()
  {
    var projectRoot = OptionalProjectRootOption();
    var file = ProjectMetadataFile();
    var command = new Command(
      "require",
      "Require that the environment contains all required variables.") { projectRoot, file };
    command.SetHandler<string?, string?>(EnvRequireEntrypoint.HandleAsync, projectRoot, file);
    return command;
  }

  private static Option OptionalProjectRootOption()
  {
    var option = ProjectRootOption.Create();
    option.IsRequired = false;
    return option;
  }

  private static Option<string> ProjectMetadataFile()
  {
    return new Option<string>(
      new[] { "--file", "-f" },
      "Project metadata file."
    ) { IsRequired = false };
  }
}
