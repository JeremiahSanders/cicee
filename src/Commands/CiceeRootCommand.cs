using System.CommandLine;
using Cicee.Commands.Env;
using Cicee.Commands.Exec;
using Cicee.Commands.Init;
using Cicee.Commands.Template;

namespace Cicee.Commands
{
  internal static class CiceeRootCommand
  {
    public static RootCommand Create()
    {
      var command = new RootCommand("cicee")
      {
        EnvCommand.Create(), ExecCommand.Create(), InitCommand.Create(), TemplateCommand.Create()
      };
      return command;
    }
  }
}
