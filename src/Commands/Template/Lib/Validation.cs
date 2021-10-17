using System.Threading.Tasks;
using Cicee.CiEnv;
using Cicee.Commands.Lib;
using LanguageExt.Common;

namespace Cicee.Commands.Template.Lib
{
  public static class Validation
  {
    public static async Task<Result<TemplateLibContext>> ValidateRequestAsync(
      CommandDependencies dependencies,
      TemplateLibRequest request
    )
    {
      var (projectRoot, shell, overwriteFiles) = request;
      return await dependencies.EnsureDirectoryExists(projectRoot)
        .BindAsync(async validatedProjectRoot =>
          (await Cicee.Commands.Lib.Validation.ValidateRequestAsync(dependencies, new LibRequest(shell ?? "")))
          .Map(libContext => new TemplateLibContext(
              validatedProjectRoot,
              libContext.ShellTemplate,
              libContext.LibPath,
              overwriteFiles
            )
          )
        );
    }
  }
}
