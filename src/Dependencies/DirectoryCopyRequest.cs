using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Cicee.Dependencies;

[ExcludeFromCodeCoverage]
public record DirectoryCopyRequest(string SourceDirectoryPath, string DestinationDirectoryPath, bool Overwrite);

[ExcludeFromCodeCoverage]
public record DirectoryCopyResult(
  string SourceDirectoryPath,
  string DestinationDirectoryPath,
  bool Overwrite,
  IReadOnlyList<(string SourceDirectory, string DestinationDirectory)> CreatedDirectories,
  IReadOnlyList<(string SourceFile, string DestinationFile)> CopiedFiles,
  IReadOnlyList<(string SourceDirectory, string DestinationDirectory)> SkippedDirectories,
  IReadOnlyList<(string SourceFile, string DestinationFile)> SkippedFiles);
