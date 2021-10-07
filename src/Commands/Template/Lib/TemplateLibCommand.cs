using System.CommandLine;
using System.CommandLine.Invocation;

namespace Cicee.Commands.Template.Lib
{
  public static class TemplateLibCommand
  {
    public static Command Create()
    {
      var command =
        new Command(
          "lib",
          "Initialize project CI with CICEE execution library. Supports 'cicee exec'-like behavior without CICEE installation.")
        {
          ProjectRootOption.Create(), ShellOption.Create(), ForceOption.Create()
        };
      command.Handler = CommandHandler.Create<string, string?, bool>(TemplateLibEntrypoint.HandleAsync);
      return command;
    }
  }
}
