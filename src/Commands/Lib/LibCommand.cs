using System.CommandLine;
using System.CommandLine.Invocation;

namespace Cicee.Commands.Lib
{
  public static class LibCommand
  {
    public static Command Create()
    {
      var command =
        new Command("lib",
          "Gets the path of the CICEE shell script library. Intended to be used as the target of 'source', i.e., 'source \"$(cicee lib --shell bash)\"'.")
        {
          ShellOption.Create()
        };
      command.Handler = CommandHandler.Create<string?>(LibEntrypoint.HandleAsync);
      return command;
    }
  }
}
