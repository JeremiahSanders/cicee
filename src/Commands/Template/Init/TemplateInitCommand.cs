using System.CommandLine;

using Cicee.Dependencies;

namespace Cicee.Commands.Template.Init;

public static class TemplateInitCommand
{
  public static Command Create(ICommandDependencies dependencies)
  {
    Option<string> projectRoot = ProjectRootOption.Create(dependencies);
    Option<bool> force = ForceOption.Create();
    Command command = new(name: "init", description: "Initialize project CI scripts.")
    {
      projectRoot, force
    };
    command.SetHandler(
      (rootValue, forceValue) => TemplateInitEntrypoint.HandleAsync(dependencies, rootValue, forceValue),
      projectRoot,
      force
    );

    return command;
  }
}
