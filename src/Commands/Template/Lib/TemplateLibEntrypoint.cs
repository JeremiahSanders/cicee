using System.Threading.Tasks;
using LanguageExt.Common;

namespace Cicee.Commands.Template.Lib
{
  public static class TemplateLibEntrypoint
  {
    /// <summary>
    ///   Handle a template library command.
    /// </summary>
    /// <param name="projectRoot">A project root.</param>
    /// <param name="shell">A library shell type.</param>
    /// <param name="force">Should we forcibly overwrite existing files?</param>
    /// <returns>An asynchronous exit code.</returns>
    public static async Task<int> HandleAsync(string projectRoot, string? shell, bool force)
    {
      var dependencies = CommandDependencies.Create();
      return (await new Result<TemplateLibRequest>(
            new TemplateLibRequest(projectRoot, shell, force))
          .BindAsync(request => TemplateLibHandling.TryHandleRequest(dependencies, request))
        )
        .ToExitCode();
    }
  }
}
