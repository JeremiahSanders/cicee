using System.CommandLine;
using Cicee.Commands.Lib;
using Cicee.Dependencies;

namespace Cicee.Commands.Template.Lib;

public static class TemplateLibCommand
{
  public const string Description =
    "Initialize project CI with CICEE execution library. Supports 'cicee exec'-like behavior without CICEE installation.";

  public static Command Create(CommandDependencies dependencies)
  {
    var projectRoot = ProjectRootOption.Create(dependencies);
    var shell = ShellOption.CreateOptional();
    var force = ForceOption.Create();
    var command =
      new Command(
        "lib",
        Description) { projectRoot, shell, force };
    command.SetHandler<string, LibraryShellTemplate?, bool>(TemplateLibEntrypoint.HandleAsync, projectRoot, shell, force);
    return command;
  }
}
