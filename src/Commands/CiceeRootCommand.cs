using System.CommandLine;
using Cicee.Commands.Exec;
using Cicee.Commands.Init;

namespace Cicee.Commands
{
  static class CiceeRootCommand
  {
    public static RootCommand Create()
    {
      var command = new RootCommand("cicee")
      {
        ExecCommand.Create(),
        InitCommand.Create(),
      };
      return command;
    }
  }
}
