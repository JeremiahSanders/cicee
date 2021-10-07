using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace Cicee.Commands.Lib
{
  public static class Validation
  {
    public static Task<Result<LibContext>> ValidateRequestAsync(CommandDependencies dependencies, LibRequest request)
    {
      LibContext CreateBashLibContext()
      {
        return new(LibraryShellTemplate.Bash, LibraryPaths.BashEntrypoint(dependencies));
      }

      return request.Shell.ToLowerInvariant().Trim() switch
      {
        "bash" => new Result<LibContext>(CreateBashLibContext()).AsTask(),
        "" => new Result<LibContext>(CreateBashLibContext()).AsTask(),
        _ => new Result<LibContext>(new BadRequestException("Unsupported shell."))
          .AsTask()
      };
    }
  }
}
