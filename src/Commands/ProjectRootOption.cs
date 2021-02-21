using System.CommandLine;
using System.IO;

namespace Cicee.Commands
{
  internal static class ProjectRootOption
  {
    public static Option Create()
    {
      return new Option<string>(
        aliases: new[] {"--project-root", "-p"},
        Directory.GetCurrentDirectory,
        description: "Project repository root directory"
      ) {IsRequired = true};
    }
  }
}
