using System;
using System.Threading.Tasks;

using Cicee.Dependencies;

namespace Cicee.Commands.Meta.Version.Bump;

public static class MetaVersionBumpEntrypoint
{
  public static Func<string, bool, SemVerIncrement, Task<int>> CreateHandler(ICommandDependencies dependencies)
  {
    return Handle;

    async Task<int> Handle(string projectMetadataPath, bool isDryRun, SemVerIncrement semVerIncrement)
    {
      return (await MetaVersionBumpHandling.Handle(dependencies, projectMetadataPath, isDryRun, semVerIncrement))
        .TapSuccess(dependencies.StandardOutWriteLine)
        .TapFailure(
          exception => { dependencies.StandardErrorWriteLine(exception.ToExecutionFailureMessage()); }
        )
        .ToExitCode();
    }
  }
}
