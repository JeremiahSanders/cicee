using System.CommandLine;

using Cicee.Commands.Lib;
using Cicee.Dependencies;

namespace Cicee.Commands.Template.Lib;

public static class TemplateLibCommand
{
  public const string Description =
    "Initialize project CI with CICEE execution library. Supports 'cicee exec'-like behavior without CICEE installation.";

  public static Command Create(ICommandDependencies dependencies)
  {
    Option<string> projectRoot = ProjectRootOption.Create(dependencies);
    Option<LibraryShellTemplate?> shell = ShellOption.CreateOptional();
    Option<bool> force = ForceOption.Create();
    Command command = new(name: "lib", Description)
    {
      projectRoot, shell, force
    };
    command.SetHandler(
      (rootValue, shellValue, forceValue) => TemplateLibEntrypoint.HandleAsync(
        dependencies,
        rootValue,
        shellValue,
        forceValue
      ),
      projectRoot,
      shell,
      force
    );

    return command;
  }
}
