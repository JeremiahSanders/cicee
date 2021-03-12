using System.CommandLine;
using Cicee.Commands.Exec;
using Cicee.Commands.Init;
using Cicee.Commands.Template;

namespace Cicee.Commands
{
  internal static class CiceeRootCommand
  {
    public static RootCommand Create()
    {
      var command = new RootCommand("cicee") {ExecCommand.Create(), InitCommand.Create(), TemplateCommand.Create()};
      return command;
    }
  }
}
