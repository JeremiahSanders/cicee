using System.Diagnostics.CodeAnalysis;

namespace Cicee.Commands.Init
{
  [ExcludeFromCodeCoverage]
  public record FileCopyRequest(string SourcePath, string DestinationPath);
}
