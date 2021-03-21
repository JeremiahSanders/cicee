using System.CommandLine;
using Cicee.Commands.Env.Require;

namespace Cicee.Commands.Env
{
  public class EnvCommand
  {
    public static Command Create()
    {
      return new Command("env", "Commands which interact with the current environment.") {EnvRequireCommand.Create()};
    }
  }
}
