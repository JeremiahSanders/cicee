using System.Diagnostics.CodeAnalysis;

namespace Cicee.Dependencies;

[ExcludeFromCodeCoverage]
public record FileCopyRequest(string SourcePath, string DestinationPath);
