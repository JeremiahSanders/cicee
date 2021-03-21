using System.Threading.Tasks;

namespace Cicee.Commands.Lib.Bash
{
  public class LibBashHandling
  {
    public static Task<int> HandleAsync(CommandDependencies dependencies)
    {
      string CreateBashLibPath()
      {
        return
          dependencies.CombinePath(
            dependencies.CombinePath(
              dependencies.CombinePath(dependencies.GetLibraryRootPath(), "ci"),
              "bash"
            ),
            "ci.sh"
          );
      }

      dependencies.StandardOutWriteLine(CreateBashLibPath());
      return Task.FromResult(result: 0);
    }
  }
}