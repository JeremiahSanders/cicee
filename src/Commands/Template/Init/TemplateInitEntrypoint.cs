using System.Threading.Tasks;

using Cicee.Dependencies;

using LanguageExt.Common;

namespace Cicee.Commands.Template.Init;

public static class TemplateInitEntrypoint
{
  /// <summary>
  ///   Handle a template initialization command.
  /// </summary>
  /// <param name="dependencies">Command dependencies.</param>
  /// <param name="projectRoot">A project root.</param>
  /// <param name="force">Should we forcibly overwrite existing files?</param>
  /// <returns>A <see cref="Result{A}" /> of <see cref="TemplateInitResult" />.</returns>
  public static async Task<Result<TemplateInitResult>> TryHandleAsync(CommandDependencies dependencies,
    string projectRoot, bool force)
  {
    return await new Result<TemplateInitRequest>(new TemplateInitRequest(projectRoot, force)).BindAsync(
      request => TemplateInitHandling.TryHandleRequest(dependencies, request)
    );
  }

  /// <summary>
  ///   Handle a template initialization command.
  /// </summary>
  /// <param name="dependencies">Command dependencies.</param>
  /// <param name="projectRoot">A project root.</param>
  /// <param name="force">Should we forcibly overwrite existing files?</param>
  /// <returns>A process exit code.</returns>
  public static async Task<int> HandleAsync(CommandDependencies dependencies, string projectRoot, bool force)
  {
    return (await TryHandleAsync(dependencies, projectRoot, force)).ToExitCode();
  }
}
