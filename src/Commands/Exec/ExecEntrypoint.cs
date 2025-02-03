using System.Threading.Tasks;

using Cicee.Dependencies;

namespace Cicee.Commands.Exec;

public static class ExecEntrypoint
{
  public static async Task<int> HandleAsync(
    ICommandDependencies dependencies,
    string projectRoot,
    string? command,
    string? entrypoint,
    string? image,
    ExecInvocationHarness harness,
    ExecVerbosity verbosity
  )
  {
    ExecHandler handler = new(dependencies);
    ExecRequest request = new(projectRoot, command, entrypoint, image, harness, verbosity);

    return (await handler.HandleAsync(request))
      .TapFailure(exception => dependencies.StandardErrorWriteLine(exception.ToExecutionFailureMessage()))
      .ToExitCode();
  }
}
