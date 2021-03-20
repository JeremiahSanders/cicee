using System.Diagnostics.CodeAnalysis;

namespace Cicee.Commands
{
  [ExcludeFromCodeCoverage]
  public record FileCopyResult(FileCopyRequest Request, bool Written);
}
