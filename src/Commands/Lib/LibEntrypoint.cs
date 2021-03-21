using System.Threading.Tasks;

namespace Cicee.Commands.Lib
{
  public static class LibEntrypoint
  {
    public static async Task<int> HandleAsync(string? shell)
    {
      var dependencies = CommandDependencies.Create();
      return (await LibHandling.HandleAsync(dependencies, new LibRequest(shell ?? string.Empty)))
        .TapLeft(exception =>
        {
          dependencies.StandardErrorWriteLine(exception.ToExecutionFailureMessage());
        })
        .ToExitCode();
    }
  }
}
