using System.Threading.Tasks;
using Cicee.Commands.Init;

namespace Cicee.Commands.Template
{
  public static class TemplateInitEntrypoint
  {
    public static async Task<int> HandleAsync(string projectRoot, bool force)
    {
      var dependencies = CommandDependencies.Create();
      return (await TemplateInitHandling.TryCreateRequest(dependencies, projectRoot, force)
          .BindAsync(request => TemplateInitHandling.TryHandleRequest(dependencies, request))
        )
        .ToExitCode();
    }
  }
}
