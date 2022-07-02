using System;
using System.Threading.Tasks;
using LanguageExt;

namespace Cicee.Commands.Meta.Version;

public static class MetaVersionEntrypoint
{
  public static Func<string, Task<int>> CreateHandler(CommandDependencies dependencies)
  {
    Task<int> Handle(string projectMetadataPath)
    {
      return MetaVersionHandling.HandleMetaVersionRequest(dependencies, projectMetadataPath)
        .Tap(dependencies.StandardOutWriteLine)
        .TapLeft(exception =>
        {
          dependencies.StandardErrorWriteLine(exception.ToExecutionFailureMessage());
        })
        .ToExitCode()
        .AsTask();
    }

    return Handle;
  }
}
