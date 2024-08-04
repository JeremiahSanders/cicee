using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

using Cicee.Commands;

namespace Cicee;

internal static class Program
{
  public static async Task<int> Main(string[] args)
  {
    RootCommand rootCommand = CiceeRootCommand.Create();
    CommandLineBuilder builder = new CommandLineBuilder(rootCommand).AddMiddleware(WelcomeMiddleware.InvokeMiddleware)
      .UseDefaults();
    Parser parser = builder.Build();
    // InvokeAsync appears to trap errors.
    int exitCode = await parser.InvokeAsync(args);
    return exitCode;
  }
}
