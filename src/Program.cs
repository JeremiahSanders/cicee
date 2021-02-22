using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Cicee.Commands;

namespace Cicee
{
  internal class Program
  {
    public static async Task<int> Main(string[] args)
    {
      var rootCommand = CiceeRootCommand.Create();
      var builder = new CommandLineBuilder(rootCommand)
        .UseDefaults()
        .UseMiddleware(WelcomeMiddleware.InvokeMiddleware);
      var parser = builder.Build();
      return await parser.InvokeAsync(args);
    }
  }
}
