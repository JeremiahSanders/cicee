using System.CommandLine;

using Cicee.Dependencies;

namespace Cicee.Commands.Env.Display;

public static class EnvDisplayCommand
{
  public static Command Create(ICommandDependencies dependencies)
  {
    Option<string> projectMetadataOption = ProjectMetadataOption.Create(dependencies);
    Command command = new(name: "display", description: "Display values of current project CI environment variables.")
    {
      projectMetadataOption
    };

    command.SetHandler(EnvDisplayEntrypoint.CreateHandler(dependencies), projectMetadataOption);

    return command;
  }
}
