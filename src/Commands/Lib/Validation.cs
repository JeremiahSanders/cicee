using System.Threading.Tasks;

using Cicee.Dependencies;

using LanguageExt;
using LanguageExt.Common;

namespace Cicee.Commands.Lib;

public static class Validation
{
  public static Task<Result<LibContext>> ValidateRequestAsync(CommandDependencies dependencies, LibRequest request)
  {
    return request.Shell switch
    {
      LibraryShellTemplate.Bash => new Result<LibContext>(CreateBashLibContext()).AsTask(),
      _ => new Result<LibContext>(new BadRequestException(message: "Unsupported shell.")).AsTask()
    };

    LibContext CreateBashLibContext()
    {
      return new LibContext(LibraryShellTemplate.Bash, LibraryPaths.BashEntrypoint(dependencies));
    }
  }
}
