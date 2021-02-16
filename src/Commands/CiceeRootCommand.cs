using System.CommandLine;
using Cicee.Commands.Exec;

namespace Cicee.Commands
{
  static class CiceeRootCommand
  {
    public static RootCommand Create()
    {
      var command = new RootCommand("cicee")
      {
        ExecCommand.Create()
      };
      return command;
    }
  }
}
