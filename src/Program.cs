using System.CommandLine;
using System.Threading.Tasks;

namespace Cicee
{
  class Program
  {
    static async Task Main(string[] args)
    {
      var rootCommand = CiceeRootCommand.Create();
      await rootCommand.InvokeAsync(args);
    }
  }
}
