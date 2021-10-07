using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Cicee.Commands.Exec;
using Cicee.Commands.Init;
using LanguageExt;
using LanguageExt.Common;

namespace Cicee.Commands
{
  [ExcludeFromCodeCoverage]
  public record CommandDependencies(
    Func<string, string, string> CombinePath,
    Func<string, Result<string>> EnsureDirectoryExists,
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
    Func<DirectoryCopyRequest, Task<Result<DirectoryCopyResult>>> TryCopyDirectoryAsync
  )
  {
    public Result<string> EnsureFileExists(string file)
    {
      return DoesFileExist(file)
        .Bind(exists =>
          exists
            ? new Result<string>(file)
            : new Result<string>(new FileNotFoundException($"File '{file}' does not exist.", file))
        );
    }

    public static CommandDependencies Create()
    {
      return new(
        Io.PathCombine2,
        Io.EnsureDirectoryExists,
        EnvironmentVariableHelpers.GetEnvironmentVariables,
        Console.Out.WriteLine,
        Console.Error.WriteLine,
        Io.TryLoadFileString,
        processStartInfo => ProcessHelpers.ExecuteProcessAsync(processStartInfo, debugLogger: null),
        Io.GetLibraryRootPath,
        Io.CopyTemplateToPath,
        Io.DoesFileExist,
        Io.GetInitTemplatesDirectoryPath,
        Io.GetFileNameForPath,
        Io.TryWriteFileStringAsync,
        Io.TryCopyDirectoryAsync
      );
    }
  }
}
