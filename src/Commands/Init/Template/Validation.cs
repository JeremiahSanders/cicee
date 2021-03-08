using LanguageExt.Common;

namespace Cicee.Commands.Init.Template
{
  public static class Validation
  {
    public static Result<TemplateRequest> ValidateRequestExecution(InitDependencies dependencies, TemplateRequest request)
    {
      return dependencies.EnsureDirectoryExists(request.ProjectRoot).Map(_ => request);
    }
  }
}
