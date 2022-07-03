using System.CommandLine;
using Cicee.Commands.Env.Display;
using Cicee.Commands.Env.Require;

namespace Cicee.Commands.Env
{
  public class EnvCommand
  {
    public static Command Create(CommandDependencies dependencies)
    {
      return new Command("env", "Commands which interact with the current environment.")
      {
        EnvRequireCommand.Create(),
        EnvDisplayCommand.Create(dependencies)
      };
    }
  }
}
