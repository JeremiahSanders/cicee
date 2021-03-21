using System.Diagnostics.CodeAnalysis;

namespace Cicee.Commands
{
  [ExcludeFromCodeCoverage]
  public record FileCopyRequest(string SourcePath, string DestinationPath);
}
