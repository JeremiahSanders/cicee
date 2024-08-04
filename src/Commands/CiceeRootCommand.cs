using System.CommandLine;

using Cicee.Commands.Env;
using Cicee.Commands.Exec;
using Cicee.Commands.Init;
using Cicee.Commands.Lib;
using Cicee.Commands.Meta;
using Cicee.Commands.Template;
using Cicee.Dependencies;

namespace Cicee.Commands;

internal static class CiceeRootCommand
{
  public static RootCommand Create()
  {
    CommandDependencies dependencies = CommandDependencies.Create();
    RootCommand command = new(description: "cicee")
    {
      EnvCommand.Create(dependencies),
      ExecCommand.Create(dependencies),
      InitCommand.Create(dependencies),
      LibCommand.Create(dependencies),
      MetaCommand.Create(dependencies),
      TemplateCommand.Create(dependencies)
    };
    return command;
  }
}
