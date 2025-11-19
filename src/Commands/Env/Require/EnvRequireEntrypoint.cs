using System.Threading.Tasks;

using Cicee.Dependencies;

using LanguageExt.Common;

namespace Cicee.Commands.Env.Require;

public static class EnvRequireEntrypoint
{
  private const int DefaultFailureExitCode = -1;

  public static async Task<int> HandleAsync(ICommandDependencies dependencies, string? projectRoot, string? file)
  {
    return (await TryCreateRequest(projectRoot, file)
      .MapAsync(request => EnvRequireHandling.TryHandleAsync(dependencies, request))).ToExitCode();
  }

  public static Result<EnvRequireRequest> TryCreateRequest(string? projectRoot, string? file)
  {
    return new Result<EnvRequireRequest>(new EnvRequireRequest(projectRoot, file));
  }
}
