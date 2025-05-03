using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Cicee.Edges;
using Cicee.Edges.Filesystem;
using Cicee.Edges.Processes;

using LanguageExt.Common;

namespace Cicee.Dependencies;

/// <summary>
///   Impure dependencies required by commands.
/// </summary>
/// <param name="CombinePath"></param>
/// <param name="EnsureDirectoryExists"></param>
/// <param name="GetEnvironmentVariables"></param>
/// <param name="StandardOutWriteLine"></param>
/// <param name="StandardErrorWriteLine"></param>
/// <param name="TryLoadFileString"></param>
/// <param name="ProcessExecutor"></param>
/// <param name="GetLibraryRootPath">Gets the CICEE assembly's <c>lib</c> content directory path.</param>
/// <param name="CopyTemplateToPath"></param>
/// <param name="DoesFileExist"></param>
/// <param name="GetInitTemplatesDirectoryPath"></param>
/// <param name="GetFileName"></param>
/// <param name="TryWriteFileStringAsync"></param>
/// <param name="TryCopyDirectoryAsync"></param>
/// <param name="TryGetCurrentDirectory"></param>
/// <param name="TryGetParentDirectory"></param>
/// <param name="StandardOutWrite"></param>
[ExcludeFromCodeCoverage]
public record CommandDependencies(
  Func<string, string, string> CombinePath,
  Func<string, Result<string>> EnsureDirectoryExists,
  Func<IReadOnlyDictionary<string, string>> GetEnvironmentVariables,
  Action<string> StandardOutWriteLine,
  Action<string> StandardErrorWriteLine,
  Func<string, Result<string>> TryLoadFileString,
  Func<ProcessStartInfo, Action<string>?, Task<Result<ProcessExecResult>>> ProcessExecutor,
  Func<string> GetLibraryRootPath,
  Func<FileCopyRequest, IReadOnlyDictionary<string, string>, Result<FileCopyRequest>> CopyTemplateToPath,
  Func<string, Result<bool>> DoesFileExist,
  Func<string> GetInitTemplatesDirectoryPath,
  Func<string, string> GetFileName,
  Func<(string FileName, string Content), Task<Result<(string FileName, string Content)>>> TryWriteFileStringAsync,
  Func<DirectoryCopyRequest, Task<Result<DirectoryCopyResult>>> TryCopyDirectoryAsync,
  Func<Result<string>> TryGetCurrentDirectory,
  Func<string, Result<string>> TryGetParentDirectory,
  Action<ConsoleColor?, string> StandardOutWrite
) : ICommandDependencies
{
  Result<string> IFileDependencies.TryLoadFileString(string filePath)
  {
    return TryLoadFileString(filePath);
  }

  Task<Result<ProcessExecResult>> IProcessDependencies.ProcessExecutor(
    ProcessExecRequest request,
    Action<string>? debugLogger)
  {
    return ProcessExecutor(request.ToProcessStartInfo(), debugLogger);
  }

  string IEnvironmentDependencies.GetLibraryRootPath()
  {
    return GetLibraryRootPath();
  }

  Result<FileCopyRequest> IFileDependencies.CopyTemplateToPath(
    FileCopyRequest request,
    IReadOnlyDictionary<string, string> templateParameters)
  {
    return CopyTemplateToPath(request, templateParameters);
  }

  Result<bool> IFileDependencies.DoesFileExist(string filePath)
  {
    return DoesFileExist(filePath);
  }

  string IEnvironmentDependencies.GetInitTemplatesDirectoryPath()
  {
    return GetInitTemplatesDirectoryPath();
  }

  string IPathDependencies.GetFileName(string path)
  {
    return GetFileName(path);
  }

  Task<Result<(string FileName, string Content)>> IFileDependencies.TryWriteFileStringAsync(
    (string FileName, string Content) file)
  {
    return TryWriteFileStringAsync(file);
  }

  Task<Result<DirectoryCopyResult>> IDirectoryDependencies.TryCopyDirectoryAsync(DirectoryCopyRequest request)
  {
    return TryCopyDirectoryAsync(request);
  }

  Result<string> IDirectoryDependencies.TryGetCurrentDirectory()
  {
    return TryGetCurrentDirectory();
  }

  Result<string> IPathDependencies.TryGetParentDirectory(string path)
  {
    return TryGetParentDirectory(path);
  }

  void IConsoleDependencies.StandardOutWrite(ConsoleColor? color, string text)
  {
    StandardOutWrite(color, text);
  }

  string IPathDependencies.CombinePath(string prefix, string suffix)
  {
    return CombinePath(prefix, suffix);
  }

  Result<string> IDirectoryDependencies.EnsureDirectoryExists(string path)
  {
    return EnsureDirectoryExists(path);
  }

  IReadOnlyDictionary<string, string> IEnvironmentDependencies.GetEnvironmentVariables()
  {
    return GetEnvironmentVariables();
  }

  void IConsoleDependencies.StandardOutWriteLine(string text)
  {
    StandardOutWriteLine(text);
  }

  void IConsoleDependencies.StandardErrorWriteLine(string text)
  {
    StandardErrorWriteLine(text);
  }

  /// <summary>
  ///   Initializes a new instance of <see cref="CommandDependencies" /> using the default environment providers.
  /// </summary>
  /// <returns></returns>
  public static CommandDependencies Create()
  {
    return new CommandDependencies(
      Io.PathCombine2,
      Io.EnsureDirectoryExists,
      EnvironmentVariableHelpers.GetEnvironmentVariables,
      Console.Out.WriteLine,
      line =>
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(line);
        Console.ResetColor();
      },
      Io.TryLoadFileString,
      ProcessHelpers.ExecuteProcessAsync,
      Io.GetLibraryRootPath,
      Io.CopyTemplateToPath,
      Io.DoesFileExist,
      Io.GetInitTemplatesDirectoryPath,
      Io.GetFileNameForPath,
      Io.TryWriteFileStringAsync,
      Io.TryCopyDirectoryAsync,
      Io.TryGetCurrentDirectory,
      Io.TryGetParentDirectory,
      (consoleColor, value) =>
      {
        if (consoleColor != null)
        {
          Console.ForegroundColor = consoleColor.Value;
        }
        else
        {
          Console.ResetColor();
        }

        Console.Out.Write(value);
        Console.ResetColor();
      }
    );
  }
}
