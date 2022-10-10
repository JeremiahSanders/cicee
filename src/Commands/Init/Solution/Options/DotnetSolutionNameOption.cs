using System.CommandLine;

namespace Cicee.Commands.Init.Solution.Options;

public static class DotnetSolutionNameOption
{
  public static Option<string?> Create()
  {
    return new Option<string?>(new[] {"--solution-name"});
  }
}
