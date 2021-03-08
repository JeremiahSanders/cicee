using System.CommandLine;
using System.CommandLine.Invocation;

namespace Cicee.Commands.Init.Template
{
  public static class TemplateCommand
  {
    public static Command Create()
    {
      var command =
        new Command("template", "Initialize project CI scripts.")
        {
          ProjectRootOption.Create(), ForceOption.Create()
        };
      command.Handler = CommandHandler.Create<string, bool>(TemplateEntrypoint.HandleAsync);
      return command;
    }
  }
}
