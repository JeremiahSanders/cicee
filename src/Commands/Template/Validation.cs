using Cicee.Commands.Init;
using LanguageExt.Common;

namespace Cicee.Commands.Template
{
  public static class Validation
  {
    public static Result<TemplateInitRequest> ValidateRequestExecution(CommandDependencies dependencies, TemplateInitRequest request)
    {
      return dependencies.EnsureDirectoryExists(request.ProjectRoot).Map(_ => request);
    }
  }
}
