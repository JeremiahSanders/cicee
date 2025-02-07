using System.CommandLine;

using Cicee.Dependencies;

namespace Cicee.Commands.Lib.Exec;

public static class LibExecCommand
{
  public const string LibExecName = "exec";

  public static string GetDescription(ICommandDependencies dependencies){
    return $"Execute a command within a shell which has the CICEE CI shell library ({dependencies.GetLibraryRootPath()}) and is initialized for the project environment.";
  }

  private static Option<string> ShellCommandOption()
  {
    return new Option<string>(
      new[]
      {
        "--command",
        "-c"
      },
      description: "Shell command"
    )
    {
      IsRequired = true
    };
  }

  public static Command Create(ICommandDependencies dependencies)
  {
    Option<LibraryShellTemplate> shellOption = ShellOption.CreateRequired();
    Option<string> commandOption = ShellCommandOption();
    Option<string> projectRootOption = ProjectRootOption.Create(dependencies);
    Option<string> metadataOption = ProjectMetadataOption.Create(dependencies);
    Command command = new(LibExecName, GetDescription(dependencies))
    {
      shellOption, commandOption, projectRootOption, metadataOption
    };

    command.SetHandler(
      (templateValue, commandValue, projectRootValue, metadataValue) => LibExecEntrypoint.HandleAsync(
        dependencies,
        templateValue,
        commandValue,
        projectRootValue,
        metadataValue
      ),
      shellOption,
      commandOption,
      projectRootOption,
      metadataOption
    );

    return command;
  }
}
