using System.CommandLine;
using System.CommandLine.Invocation;

namespace Cicee.Commands.Lib.Bash
{
  public static class LibBashCommand
  {
    public static Command Create()
    {
      var command =
        new Command("bash",
          "Gets the path of the Bash CI library. Intended to be used as the target of 'source', i.e., 'source \"$(cicee lib bash)\"'.");
      command.Handler = CommandHandler.Create(LibBashEntrypoint.HandleAsync);
      return command;
    }
  }
}
