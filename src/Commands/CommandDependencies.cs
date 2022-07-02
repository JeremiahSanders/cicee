using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Cicee.Commands.Exec;
using LanguageExt.Common;

namespace Cicee.Commands;

[ExcludeFromCodeCoverage]
public record CommandDependencies(
  Func<string, string, string> CombinePath,
  Func<string, Result<string>> EnsureDirectoryExists,
  Func<string, Result<string>> EnsureFileExists,
  Func<IReadOnlyDictionary<string, string>> GetEnvironmentVariables,
  Action<string> StandardOutWriteLine,
  Action<string> StandardErrorWriteLine,
  Func<string, Result<string>> TryLoadFileString,
  Func<ProcessStartInfo, Task<Result<ProcessExecResult>>> ProcessExecutor,
  Func<string> GetLibraryRootPath,
  Func<FileCopyRequest, IReadOnlyDictionary<string, string>, Result<FileCopyRequest>> CopyTemplateToPath,
  Func<string, Result<bool>> DoesFileExist,
  Func<string> GetInitTemplatesDirectoryPath,
  Func<string, string> GetFileName,
  Func<(string FileName, string Content), Task<Result<(string FileName, string Content)>>> TryWriteFileStringAsync,
  Func<DirectoryCopyRequest, Task<Result<DirectoryCopyResult>>> TryCopyDirectoryAsync,
  Func<Result<string>> TryGetCurrentDirectory,
  Func<string, Result<string>> TryGetParentDirectory
)
{
  public static CommandDependencies Create()
  {
    return new CommandDependencies(
      Io.PathCombine2,
      Io.EnsureDirectoryExists,
      Io.EnsureFileExists,
      EnvironmentVariableHelpers.GetEnvironmentVariables,
      Console.Out.WriteLine,
      line =>
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(line);
        Console.ResetColor();
      },
      Io.TryLoadFileString,
      processStartInfo => ProcessHelpers.ExecuteProcessAsync(processStartInfo, debugLogger: null),
      Io.GetLibraryRootPath,
      Io.CopyTemplateToPath,
      Io.DoesFileExist,
      Io.GetInitTemplatesDirectoryPath,
      Io.GetFileNameForPath,
      Io.TryWriteFileStringAsync,
      Io.TryCopyDirectoryAsync,
      Io.TryGetCurrentDirectory,
      Io.TryGetParentDirectory
    );
  }
}
