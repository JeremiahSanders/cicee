using System.CommandLine;
using Cicee.Commands.Template.Init;
using Cicee.Commands.Template.Lib;

namespace Cicee.Commands.Template
{
  public static class TemplateCommand
  {
    public static Command Create()
    {
      var command =
        new Command("template", "Commands working with project continuous integration templates.");
      command.AddCommand(TemplateInitCommand.Create());
      command.AddCommand(TemplateLibCommand.Create());
      return command;
    }
  }
}
