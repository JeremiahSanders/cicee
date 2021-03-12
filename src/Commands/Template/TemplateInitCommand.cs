using System.CommandLine;
using System.CommandLine.Invocation;

namespace Cicee.Commands.Template
{
  public static class TemplateInitCommand
  {
    public static Command Create()
    {
      var command =
        new Command("init", "Initialize project CI scripts.") {ProjectRootOption.Create(), ForceOption.Create()};
      command.Handler = CommandHandler.Create<string, bool>(TemplateInitEntrypoint.HandleAsync);
      return command;
    }
  }
}
