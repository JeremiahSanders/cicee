using System.Collections.Generic;
using System.Threading.Tasks;

using LanguageExt.Common;

namespace Cicee.Dependencies;

/// <summary>
///   Dependencies which provide access to file system files.
/// </summary>
public interface IFileDependencies
{
  /// <summary>Attempt to load a file identified by path.</summary>
  Result<string> TryLoadFileString(string filePath);

  /// <summary>Attempts to check to see if a file exists.</summary>
  Result<bool> DoesFileExist(string filePath);

  /// <summary>Attempt to copy a template file with replacement tokens.</summary>
  Result<FileCopyRequest> CopyTemplateToPath(
    FileCopyRequest request,
    IReadOnlyDictionary<string, string> templateParameters
  );

  /// <summary>Attempts to write a string to a file.</summary>
  Task<Result<(string FileName, string Content)>> TryWriteFileStringAsync((string FileName, string Content) file);
}
