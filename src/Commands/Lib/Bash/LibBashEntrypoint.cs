using System.Threading.Tasks;

namespace Cicee.Commands.Lib.Bash
{
  public class LibBashEntrypoint
  {
    public static Task<int> HandleAsync()
    {
      return LibBashHandling.HandleAsync(CommandDependencies.Create());
    }
  }
}