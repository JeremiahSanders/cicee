using System;
using System.Threading.Tasks;

using Cicee.Dependencies;

using LanguageExt;

namespace Cicee.Commands.Meta.Version;

public static class MetaVersionEntrypoint
{
  public static Func<string, Task<int>> CreateHandler(ICommandDependencies dependencies)
  {
    return Handle;

    Task<int> Handle(string projectMetadataPath)
    {
      return MetaVersionHandling
        .HandleMetaVersionRequest(dependencies, projectMetadataPath)
        .TapSuccess(dependencies.StandardOutWriteLine)
        .TapFailure(
          exception => { dependencies.StandardErrorWriteLine(exception.ToExecutionFailureMessage()); }
        )
        .ToExitCode()
        .AsTask();
    }
  }
}
