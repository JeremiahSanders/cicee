using System.Threading.Tasks;

using Cicee.Commands.Lib;
using Cicee.Dependencies;

using LanguageExt.Common;

namespace Cicee.Commands.Template.Lib;

public static class Validation
{
  public static async Task<Result<TemplateLibContext>> ValidateRequestAsync(CommandDependencies dependencies,
    TemplateLibRequest request)
  {
    (string projectRoot, LibraryShellTemplate? shell, bool overwriteFiles) = request;
    return await dependencies.EnsureDirectoryExists(projectRoot).BindAsync(
      async validatedProjectRoot =>
        (await Commands.Lib.Validation.ValidateRequestAsync(dependencies, new LibRequest(shell))).Map(
          libContext => new TemplateLibContext(
            validatedProjectRoot,
            libContext.ShellTemplate,
            libContext.LibPath,
            overwriteFiles
          )
        )
    );
  }
}
