using System.Threading.Tasks;

namespace Cicee.Commands.Init.Template
{
  public static class TemplateEntrypoint
  {
    public static async Task<int> HandleAsync(string projectRoot, bool force)
    {
      var dependencies = InitDependencies.Create();
      return (await TemplateHandling.TryCreateRequest(dependencies, projectRoot, force)
          .BindAsync(request => TemplateHandling.TryHandleRequest(dependencies, request))
        )
        .ToExitCode();
    }
  }
}
