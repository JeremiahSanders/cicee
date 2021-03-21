using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace Cicee.Commands.Lib
{
  public static class LibHandling
  {
    public static async Task<Result<LibContext>> HandleAsync(CommandDependencies dependencies, LibRequest libRequest)
    {
      return (await ValidateRequestAsync(dependencies, libRequest))
        .Tap(context =>
        {
          dependencies.StandardOutWriteLine(context.LibPath);
        });
    }

    private static Task<Result<LibContext>> ValidateRequestAsync(CommandDependencies dependencies, LibRequest request)
    {
      LibContext CreateBashLibContext()
      {
        return new(LibraryShellTemplate.Bash,
          dependencies.CombinePath(
            dependencies.CombinePath(
              dependencies.CombinePath(dependencies.GetLibraryRootPath(), "ci"),
              "bash"
            ),
            "ci.sh"
          )
        );
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
