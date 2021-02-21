using System.Diagnostics.CodeAnalysis;

namespace Cicee.Commands.Init
{
  [ExcludeFromCodeCoverage]
  public record FileCopyResult(FileCopyRequest Request, bool Written);
}
