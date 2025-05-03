using LanguageExt.Common;

namespace Cicee.Dependencies;

/// <summary>
///   Dependencies which provide access to file system paths.
/// </summary>
public interface IPathDependencies
{
  /// <summary>Gets a filename from the provided path.</summary>
  string GetFileName(string path);

  /// <summary>Attempts to get the parent directory.</summary>
  Result<string> TryGetParentDirectory(string path);

  /// <summary>Combines two path segments.</summary>
  string CombinePath(string prefix, string suffix);
}
