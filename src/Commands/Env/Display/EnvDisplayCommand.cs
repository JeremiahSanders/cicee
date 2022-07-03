using System.CommandLine;

namespace Cicee.Commands.Env.Display;

public static class EnvDisplayCommand
{
  public static Command Create(CommandDependencies dependencies)
  {
    var projectMetadataOption = ProjectMetadataOption.Create(dependencies);
    var command =
      new Command("display", "Display values of current project CI environment variables.") {projectMetadataOption};

    command.SetHandler(EnvDisplayEntrypoint.CreateHandler(dependencies), projectMetadataOption);

    return command;
  }
}
