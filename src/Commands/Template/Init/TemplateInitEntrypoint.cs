using System.Threading.Tasks;
using LanguageExt.Common;

namespace Cicee.Commands.Template.Init
{
  public static class TemplateInitEntrypoint
  {
    /// <summary>
    ///   Handle a template initialization command.
    /// </summary>
    /// <param name="projectRoot">A project root.</param>
    /// <param name="force">Should we forcibly overwrite existing files?</param>
    /// <param name="file">A metadata file name.</param>
    /// <param name="name">A project name.</param>
    /// <param name="title">A project title.</param>
    /// <param name="description">A project description.</param>
    /// <param name="version">A project version.</param>
    /// <returns></returns>
    public static async Task<int> HandleAsync(string projectRoot, bool force, string? file, string? name, string? title,
      string? description, string? version)
    {
      var dependencies = CommandDependencies.Create();
      return (await new Result<TemplateInitRequest>(
            new TemplateInitRequest(projectRoot, force, file, name, title, description, version))
          .BindAsync(request => TemplateInitHandling.TryHandleRequest(dependencies, request))
        )
        .ToExitCode();
    }
  }
}
