using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using LanguageExt.Common;

namespace Cicee.Dependencies;

/// <summary>
///   Dependencies (e.g., I/O) required for command processing.
/// </summary>
public interface ICommandDependencies
{
  /// <summary>Attempt to load a file identified by path.</summary>
  Result<string> TryLoadFileString(string filePath);

  /// <summary>Attempt to invoke a process and await its completion.</summary>
  Task<Result<ProcessExecResult>> ProcessExecutor(ProcessExecRequest processStartInfo);

  /// <summary>Gets the CICEE assembly's <c>lib</c> content directory path.</summary>
  string GetLibraryRootPath();

  /// <summary>Attempt to copy a template file with replacement tokens.</summary>
  Result<FileCopyRequest> CopyTemplateToPath(
    FileCopyRequest request,
    IReadOnlyDictionary<string, string> templateParameters
  );

  /// <summary>Attempts to check to see if a file exists.</summary>
  Result<bool> DoesFileExist(string filePath);

  /// <summary>Gets the <c>init templates</c> command's directory path.</summary>
  string GetInitTemplatesDirectoryPath();

  /// <summary>Gets a filename from the provided path.</summary>
  string GetFileName(string path);

  /// <summary>Attempts to write a string to a file.</summary>
  Task<Result<(string FileName, string Content)>> TryWriteFileStringAsync((string FileName, string Content) file);

  /// <summary>Attempts to copy a directory.</summary>
  Task<Result<DirectoryCopyResult>> TryCopyDirectoryAsync(DirectoryCopyRequest request);

  /// <summary>Attempts to get the current directory.</summary>
  Result<string> TryGetCurrentDirectory();

  /// <summary>Attempts to get the parent directory.</summary>
  Result<string> TryGetParentDirectory(string path);

  /// <summary>Writes to standard out (or the conceptual equivalent used by the implementation).</summary>
  void StandardOutWrite(ConsoleColor? color, string text);

  /// <summary>Combines two path segments.</summary>
  string CombinePath(string prefix, string suffix);

  /// <summary>Requires that a directory exists, identified by path. Success indicates confirmation.</summary>
  Result<string> EnsureDirectoryExists(string directory);

  /// <summary>Requires that a file exists, identified by path. Success indicates confirmation.</summary>
  Result<string> EnsureFileExists(string filePath);

  /// <summary>Gets the current environment variables.</summary>
  IReadOnlyDictionary<string, string> GetEnvironmentVariables();

  /// <summary>
  ///   Writes <paramref name="text" /> to standard out (or the conceptual equivalent used by the implementation) with
  ///   a trailing <see cref="Environment.NewLine" />.
  /// </summary>
  void StandardOutWriteLine(string text);

  /// <summary>
  ///   Writes <paramref name="text" /> to standard error (or the conceptual equivalent used by the implementation)
  ///   with a trailing <see cref="Environment.NewLine" />.
  /// </summary>
  void StandardErrorWriteLine(string text);
}
