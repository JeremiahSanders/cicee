using System.Threading.Tasks;
using Cicee.Dependencies;
using LanguageExt.Common;

namespace Cicee.Commands.Init;

public static class InitEntrypoint
{
  public static async Task<Result<InitRequest>> TryHandleAsync(string projectRoot, string? image, bool force)
  {
    var dependencies = CommandDependencies.Create();
    return await InitHandling.TryCreateRequest(dependencies, projectRoot, image, force)
      .BindAsync(request => InitHandling.TryHandleRequest(dependencies, request));
  }

  public static async Task<int> HandleAsync(string projectRoot, string? image, bool force)
  {
    return (await TryHandleAsync(projectRoot, image, force)).ToExitCode();
  }
}
