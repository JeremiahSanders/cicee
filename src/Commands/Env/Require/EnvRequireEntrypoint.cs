using System.Threading.Tasks;
using LanguageExt.Common;

namespace Cicee.Commands.Env.Require
{
  public static class EnvRequireEntrypoint
  {
    private const int DefaultFailureExitCode = -1;

    public static async Task<int> HandleAsync(string? projectRoot, string? file)
    {
      return (await TryCreateRequest(projectRoot, file)
          .MapAsync(request =>
            EnvRequireHandling.TryHandleAsync(CommandDependencies.Create(), request)
          ))
        .ToExitCode();
    }

    public static Result<EnvRequireRequest> TryCreateRequest(string? projectRoot, string? file)
    {
      return new(new EnvRequireRequest(projectRoot, file));
    }
  }
}
