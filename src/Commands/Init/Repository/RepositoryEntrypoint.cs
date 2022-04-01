using System.Threading.Tasks;
using Cicee.Commands.Template.Init;
using Cicee.Commands.Template.Lib;
using LanguageExt.Common;

namespace Cicee.Commands.Init.Repository;

public static class RepositoryEntrypoint
{
  public static async Task<Result<RepositoryResult>> TryHandleAsync(string projectRoot, string? image, bool force,
    bool initCiLib, string shell)
  {
    return await (await InitEntrypoint.TryHandleAsync(projectRoot, image, force))
      .BindAsync(async initResult =>
        await (await TemplateInitEntrypoint.TryHandleAsync(projectRoot, force))
          .BindAsync(
            async templateInitResult =>
              initCiLib
                ? (await TemplateLibEntrypoint.TryHandleAsync(projectRoot, shell, force)).Map(templateLibResult =>
                  new RepositoryResult(initResult, templateInitResult) { TemplateLibResult = templateLibResult })
                : new Result<RepositoryResult>(new RepositoryResult(initResult, templateInitResult))
          ));
  }

  public static async Task<int> HandleAsync(string projectRoot, string? image, bool force, bool initCiLib, string shell)
  {
    return (await TryHandleAsync(projectRoot, image, force, initCiLib, shell)).ToExitCode();
  }
}
