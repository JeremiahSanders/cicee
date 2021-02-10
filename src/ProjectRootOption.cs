using System.CommandLine;
using System.IO;

namespace Cicee
{
  internal static class ProjectRootOption
  {
    public static Option Create()
    {
      return new Option<string>(
        new[] {"--project-root", "-p"},
        Directory.GetCurrentDirectory,
        "Project repository root directory"
      ) {IsRequired = true};
    }
  }
}
