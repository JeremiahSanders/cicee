using System.CommandLine;
using Cicee.Commands.Env;
using Cicee.Commands.Exec;
using Cicee.Commands.Init;
using Cicee.Commands.Lib;
using Cicee.Commands.Meta;
using Cicee.Commands.Template;

namespace Cicee.Commands;

internal static class CiceeRootCommand
{
  public static RootCommand Create()
  {
    var dependencies = CommandDependencies.Create();
    var command = new RootCommand("cicee")
    {
      EnvCommand.Create(),
      ExecCommand.Create(),
      InitCommand.Create(),
      LibCommand.Create(),
      MetaCommand.Create(dependencies),
      TemplateCommand.Create()
    };
    return command;
  }
}
