using System.CommandLine;

namespace Cicee.Commands.Init.Solution.Options;

public static class DotnetNamespacePrefixOption
{
  public static Option<string?> Create()
  {
    return new Option<string?>(new[] {"--namespace-prefix"});
  }
}
