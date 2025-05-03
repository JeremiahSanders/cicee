using System;
using System.Threading.Tasks;

using Cicee.Dependencies;

namespace Cicee.Commands.Lib;

public static class LibEntrypoint
{
  public static Func<LibraryShellTemplate?, Task<int>> CreateHandler(ICommandDependencies dependencies)
  {
    return shell => HandleAsync(dependencies, new LibRequest(shell));
  }

  public static async Task<int> HandleAsync(ICommandDependencies dependencies, LibRequest request)
  {
    return (await LibHandling.HandleAsync(dependencies, request))
      .TapFailure(
        exception => { dependencies.StandardErrorWriteLine(exception.ToExecutionFailureMessage()); }
      )
      .ToExitCode();
  }
}
