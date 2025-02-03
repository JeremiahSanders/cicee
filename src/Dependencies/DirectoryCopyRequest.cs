using System.Diagnostics.CodeAnalysis;

namespace Cicee.Dependencies;

[ExcludeFromCodeCoverage]
public record DirectoryCopyRequest(string SourceDirectoryPath, string DestinationDirectoryPath, bool Overwrite);