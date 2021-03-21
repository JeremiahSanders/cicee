using System.CommandLine;
using Cicee.Commands.Lib.Bash;

namespace Cicee.Commands.Lib
{
  public static class LibCommand
  {
    public static Command Create()
    {
      var command =
        new Command("lib", "Commands working with CICEE shell script library.");
      command.AddCommand(LibBashCommand.Create());
      return command;
    }
  }
}
