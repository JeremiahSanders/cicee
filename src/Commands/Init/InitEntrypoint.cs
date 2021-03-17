using System.Threading.Tasks;

namespace Cicee.Commands.Init
{
  public static class InitEntrypoint
  {
    public static async Task<int> HandleAsync(string projectRoot, string? image, bool force)
    {
      var dependencies = CommandDependencies.Create();
      return (await InitHandling.TryCreateRequest(dependencies, projectRoot, image, force)
          .BindAsync(request => InitHandling.TryHandleRequest(dependencies, request))
        )
        .ToExitCode();
    }
  }
}
