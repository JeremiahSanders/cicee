using System.CommandLine;
using Cicee.Dependencies;

namespace Cicee.Commands.Lib.Exec;

public static class LibExecCommand
{
  public const string LibExecName = "exec";

  public const string LibExecDescription =
    "Execute a command within a shell which has the CICEE CI shell library and is initialized for the project environment.";

  private static Option<string> ShellCommandOption()
  {
    return new Option<string>(
      new[] { "--command", "-c" },
      "Shell command"
    ) { IsRequired = true };
  }
  public static Command Create(CommandDependencies dependencies)
  {
    var shellOption = ShellOption.CreateRequired();
    var commandOption = ShellCommandOption();
    var projectRootOption = ProjectRootOption.Create(dependencies);
    var metadataOption = ProjectMetadataOption.Create(dependencies, true);
    var command = new Command(LibExecName, LibExecDescription) {shellOption, commandOption, projectRootOption, metadataOption};

    command.SetHandler(LibExecEntrypoint.CreateHandler(dependencies), shellOption, commandOption, projectRootOption, metadataOption);

    return command;
  }
}
