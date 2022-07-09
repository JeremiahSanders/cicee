using System.CommandLine;
using Cicee.Commands.Lib.Exec;
using Cicee.Dependencies;

namespace Cicee.Commands.Lib;

public static class LibCommand
{
  public static Command Create(CommandDependencies dependencies)
  {
    var shellOption = ShellOption.CreateOptional();
    var command =
      new Command("lib",
        "Gets the path of the CICEE shell script library. Intended to be used as the target of 'source', i.e., 'source \"$(cicee lib --shell bash)\"'.")
      {
        shellOption
      };
    command.SetHandler<LibraryShellTemplate?>(LibEntrypoint.CreateHandler(dependencies), shellOption);
    
    command.AddCommand(LibExecCommand.Create(dependencies));
    
    return command;
  }
}
