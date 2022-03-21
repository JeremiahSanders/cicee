using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Cicee.Commands;

namespace Cicee;

internal static class Program
{
  public static async Task<int> Main(string[] args)
  {
    var rootCommand = CiceeRootCommand.Create();
    var builder = new CommandLineBuilder(rootCommand)
      .AddMiddleware(WelcomeMiddleware.InvokeMiddleware)
      .UseDefaults();
    var parser = builder.Build();
    // InvokeAsync appears to trap errors.
    var exitCode = await parser.InvokeAsync(args);
    return exitCode;
  }
}
