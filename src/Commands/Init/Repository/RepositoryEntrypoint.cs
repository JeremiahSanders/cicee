using System.Threading.Tasks;

using Cicee.Commands.Lib;
using Cicee.Commands.Template.Init;
using Cicee.Commands.Template.Lib;
using Cicee.Dependencies;

using LanguageExt.Common;

namespace Cicee.Commands.Init.Repository;

public static class RepositoryEntrypoint
{
  public static async Task<Result<RepositoryResult>> TryHandleAsync(ICommandDependencies dependencies,
    string projectRoot, string? image, bool force, bool initCiLib, LibraryShellTemplate? shell)
  {
    return await (await InitEntrypoint.TryHandleAsync(dependencies, projectRoot, image, force)).BindAsync(
      async initResult =>
        await (await TemplateInitEntrypoint.TryHandleAsync(dependencies, projectRoot, force)).BindAsync(
          async templateInitResult => initCiLib
            ? (await TemplateLibEntrypoint.TryHandleAsync(dependencies, projectRoot, shell, force)).Map(
              templateLibResult => new RepositoryResult(initResult, templateInitResult)
              {
                TemplateLibResult = templateLibResult
              }
            )
            : new Result<RepositoryResult>(new RepositoryResult(initResult, templateInitResult))
        )
    );
  }

  public static async Task<int> HandleAsync(ICommandDependencies dependencies, string projectRoot, string? image,
    bool force, bool initCiLib, LibraryShellTemplate? shell)
  {
    return (await TryHandleAsync(dependencies, projectRoot, image, force, initCiLib, shell)).ToExitCode();
  }
}
