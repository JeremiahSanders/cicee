using Cicee.Dependencies;

using LanguageExt.Common;

namespace Cicee.Commands.Init;

public static class Validation
{
  public static Result<InitRequest> ValidateRequestExecution(ICommandDependencies dependencies, InitRequest request)
  {
    return dependencies
      .EnsureDirectoryExists(request.ProjectRoot)
      .Map(_ => request);
  }
}
