using System.CommandLine;

namespace Cicee.Commands.Template
{
  public static class TemplateCommand
  {
    public static Command Create()
    {
      var command =
        new Command("template", "Commands working with project continuous integration templates.");
      command.AddCommand(TemplateInitCommand.Create());
      return command;
    }
  }
}
