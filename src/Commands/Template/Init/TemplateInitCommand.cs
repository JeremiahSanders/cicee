using System.CommandLine;
using Cicee.Dependencies;

namespace Cicee.Commands.Template.Init;

public static class TemplateInitCommand
{
  public static Command Create(CommandDependencies dependencies)
  {
    var projectRoot = ProjectRootOption.Create(dependencies);
    var force = ForceOption.Create();
    var command =
      new Command("init", "Initialize project CI scripts.") { projectRoot, force };
    command.SetHandler<string, bool>(
      TemplateInitEntrypoint.HandleAsync,
      projectRoot,
      force
    );
    return command;
  }
}
