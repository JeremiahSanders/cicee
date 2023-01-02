using System.CommandLine;
using Cicee.Commands.Meta.CiEnv;
using Cicee.Commands.Meta.Version;
using Cicee.Dependencies;

namespace Cicee.Commands.Meta;

public static class MetaCommand
{
  public static Command Create(CommandDependencies dependencies)
  {
    var command = new Command("meta", "Commands interacting with project metadata.");

    command.AddCommand(CiEnvironmentCommand.Create(dependencies));
    command.AddCommand(MetaVersionCommand.Create(dependencies));

    return command;
  }
}
