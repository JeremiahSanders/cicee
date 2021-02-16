using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Cicee.Commands.Exec
{
  [ExcludeFromCodeCoverage]
  public record ProcessExecResult
  {
    public int ExitCode { get; init; }
    public IReadOnlyDictionary<string, string> Environment { get; init; } = new Dictionary<string, string>();
  }
}
