using System.CommandLine;
using System.CommandLine.Invocation;
using Cicee.Commands.Env.Require;
using Cicee.Commands.Exec;

namespace Cicee.Commands.Env
{
  public class EnvCommand
  {
    public static Command Create()
    {
      var command =
        new Command("env", "Commands which interact with the current environment.") {EnvRequireCommand.Create()};
      return command;
    }
  }
}
