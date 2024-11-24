using System.Diagnostics.CodeAnalysis;

namespace Cicee.Dependencies;

[ExcludeFromCodeCoverage]
public record FileCopyResult(FileCopyRequest Request, bool Written);
