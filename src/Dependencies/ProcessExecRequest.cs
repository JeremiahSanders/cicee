using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Cicee.Dependencies;

[ExcludeFromCodeCoverage]
public record ProcessExecRequest
{
  public string? FileName { get; init; }
  public string? Arguments { get; init; }
  public string WorkingDirectory { get; init; } = string.Empty;
  public bool UseShellExecute { get; init; }
  public IDictionary<string, string?> Environment { get; init; } = new Dictionary<string, string?>();
}
