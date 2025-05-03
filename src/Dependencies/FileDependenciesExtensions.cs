using System.IO;

using LanguageExt.Common;

namespace Cicee.Dependencies;

/// <summary>
///   Methods extending <see cref="IFileDependencies" />.´ƒ
/// </summary>
public static class FileDependenciesExtensions
{
  /// <summary>Requires that a file exists, identified by its path. Success indicates confirmation.</summary>
  public static Result<string> EnsureFileExists(this IFileDependencies fileDependencies, string file)
  {
    return fileDependencies
      .DoesFileExist(file)
      .Bind(exists => exists
        ? new Result<string>(file)
        : new Result<string>(new FileNotFoundException($"File '{file}' does not exist.", file))
      );
  }
}
