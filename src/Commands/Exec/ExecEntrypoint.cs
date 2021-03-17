using System.Threading.Tasks;

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
        .TapLeft(exception =>
        {
          dependencies.StandardErrorWriteLine($"Execution failed!\nReason: {exception.Message}");
        })
        .ToExitCode();
    }
  }
}
