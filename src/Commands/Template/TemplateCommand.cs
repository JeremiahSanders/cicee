using System.CommandLine;

using Cicee.Commands.Template.Init;
using Cicee.Commands.Template.Lib;
using Cicee.Dependencies;

namespace Cicee.Commands.Template;

public static class TemplateCommand
{
  public static Command Create(ICommandDependencies dependencies)
  {
    Command command = new(
      name: "template",
      description: "Commands working with project continuous integration templates."
    );
    command.AddCommand(TemplateInitCommand.Create(dependencies));
    command.AddCommand(TemplateLibCommand.Create(dependencies));

    return command;
  }
}
