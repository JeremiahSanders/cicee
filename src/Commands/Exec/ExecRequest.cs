using System.Diagnostics.CodeAnalysis;

namespace Cicee.Commands.Exec
{
  [ExcludeFromCodeCoverage]
  public record ExecRequest(
    string ProjectRoot,
    string? Command,
    string? Entrypoint,
    string? Image
  );
}
