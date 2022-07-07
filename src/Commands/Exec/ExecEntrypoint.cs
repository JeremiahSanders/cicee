using System.Threading.Tasks;
using Cicee.Dependencies;

namespace Cicee.Commands.Exec
{
  public static class ExecEntrypoint
  {
    public static async Task<int> HandleAsync(string projectRoot, string? command, string? entrypoint, string? image)
    {
      var dependencies = CommandDependencies.Create();

      return (await ExecHandling.HandleAsync(
          dependencies,
          new ExecRequest(
            projectRoot,
            command,
            entrypoint,
            image
          )
        ))
        .TapFailure(exception =>
        {
          dependencies.StandardErrorWriteLine(exception.ToExecutionFailureMessage());
        })
        .ToExitCode();
    }
  }
}
