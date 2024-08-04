using System.Threading.Tasks;

using Cicee.Commands.Lib;
using Cicee.Dependencies;

using LanguageExt.Common;

namespace Cicee.Commands.Template.Lib;

public static class TemplateLibEntrypoint
{
  /// <summary>
  ///   Handle a template library command.
  /// </summary>
  /// <param name="dependencies">Command dependencies.</param>
  /// <param name="projectRoot">A project root.</param>
  /// <param name="shell">A library shell type.</param>
  /// <param name="force">Should we forcibly overwrite existing files?</param>
  /// <returns>An asynchronous exit code.</returns>
  public static async Task<int> HandleAsync(CommandDependencies dependencies, string projectRoot,
    LibraryShellTemplate? shell, bool force)
  {
    return (await TryHandleAsync(dependencies, projectRoot, shell, force)).ToExitCode();
  }

  /// <summary>
  ///   Handle a template library command.
  /// </summary>
  /// <param name="dependencies">Command dependencies.</param>
  /// <param name="projectRoot">A project root.</param>
  /// <param name="shell">A library shell type.</param>
  /// <param name="force">Should we forcibly overwrite existing files?</param>
  /// <returns>An asynchronous <see cref="Result{A}" />.</returns>
  public static async Task<Result<TemplateLibResult>> TryHandleAsync(CommandDependencies dependencies,
    string projectRoot, LibraryShellTemplate? shell, bool force)
  {
    return await new Result<TemplateLibRequest>(new TemplateLibRequest(projectRoot, shell, force)).BindAsync(
      request => TemplateLibHandling.TryHandleRequest(dependencies, request)
    );
  }
}
