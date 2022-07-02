using System.Threading.Tasks;

namespace Cicee.Commands.Lib;

public static class LibEntrypoint
{
  public static Task<int> HandleAsync(string? shell)
  {
    return HandleAsync(new LibRequest(shell ?? string.Empty));
  }

  public static async Task<int> HandleAsync(LibRequest request)
  {
    var dependencies = CommandDependencies.Create();
    return (await LibHandling.HandleAsync(dependencies, request))
      .TapFailure(exception =>
      {
        dependencies.StandardErrorWriteLine(exception.ToExecutionFailureMessage());
      })
      .ToExitCode();
  }
}
