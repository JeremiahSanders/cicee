using System.Threading.Tasks;

using LanguageExt.Common;

namespace Cicee.Dependencies;

/// <summary>
///   Dependencies which provide access to file system directories.
/// </summary>
public interface IDirectoryDependencies
{
  /// <summary>Attempts to copy a directory.</summary>
  Task<Result<DirectoryCopyResult>> TryCopyDirectoryAsync(DirectoryCopyRequest request);

  /// <summary>Attempts to get the current directory.</summary>
  Result<string> TryGetCurrentDirectory();

  /// <summary>Requires that a directory exists, identified by path. Success indicates confirmation.</summary>
  Result<string> EnsureDirectoryExists(string directory);
}
