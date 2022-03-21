using System.CommandLine;

namespace Cicee.Commands.Lib;

public static class LibCommand
{
  public static Command Create()
  {
    var shellOption = ShellOption.Create();
    var command =
      new Command("lib",
        "Gets the path of the CICEE shell script library. Intended to be used as the target of 'source', i.e., 'source \"$(cicee lib --shell bash)\"'.")
      {
        shellOption
      };
    command.SetHandler<string>(LibEntrypoint.HandleAsync, shellOption);
    return command;
  }
}
