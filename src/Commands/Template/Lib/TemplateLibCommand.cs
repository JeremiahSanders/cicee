using System.CommandLine;

namespace Cicee.Commands.Template.Lib;

public static class TemplateLibCommand
{
  public static Command Create()
  {
    var projectRoot = ProjectRootOption.Create();
    var shell = ShellOption.Create();
    var force = ForceOption.Create();
    var command =
      new Command(
        "lib",
        "Initialize project CI with CICEE execution library. Supports 'cicee exec'-like behavior without CICEE installation.")
      {
        projectRoot, shell, force
      };
    command.SetHandler<string, string?, bool>(TemplateLibEntrypoint.HandleAsync, projectRoot, shell, force);
    return command;
  }
}
