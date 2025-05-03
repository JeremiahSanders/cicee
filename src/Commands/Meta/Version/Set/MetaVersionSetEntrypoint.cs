using System;
using System.Threading.Tasks;

using Cicee.Dependencies;

namespace Cicee.Commands.Meta.Version.Set;

public static class MetaVersionSetEntrypoint
{
  public static Func<string, bool, System.Version?, Task<int>> CreateHandler(ICommandDependencies dependencies)
  {
    return Handle;

    async Task<int> Handle(string projectMetadataPath, bool isDryRun, System.Version? version)
    {
      // NOTE: Use of version! should be safe due to parameter being required.
      return (await MetaVersionSetHandling.Handle(dependencies, projectMetadataPath, isDryRun, version!))
        .TapSuccess(dependencies.StandardOutWriteLine)
        .TapFailure(
          exception => { dependencies.StandardErrorWriteLine(exception.ToExecutionFailureMessage()); }
        )
        .ToExitCode();
    }
  }
}
