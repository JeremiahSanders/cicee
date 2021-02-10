using System.CommandLine;
using Cicee.Exec;

namespace Cicee
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
