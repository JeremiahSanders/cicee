using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using LanguageExt.Common;

namespace Cicee.Dependencies;

/// <summary>
///   Impure dependencies required by commands.
/// </summary>
/// <param name="CombinePath"></param>
/// <param name="EnsureDirectoryExists"></param>
/// <param name="EnsureFileExists"></param>
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
  Func<string, Result<string>> TryGetParentDirectory,
  Action<ConsoleColor?, string> StandardOutWrite
)
{
  /// <summary>
  ///   Initializes a new instance of <see cref="CommandDependencies" /> using the default environment providers.
  /// </summary>
  /// <returns></returns>
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

  public void StandardOutWriteAll(IEnumerable<(ConsoleColor? OptionalColor, string Value)> items)
  {
    foreach ((ConsoleColor? OptionalColor, string Value) tuple in items)
    {
      StandardOutWrite(tuple.OptionalColor, tuple.Value);
    }
  }

  public void StandardOutWriteAsLine(IEnumerable<(ConsoleColor? OptionalColor, string Value)> items)
  {
    StandardOutWriteAll(items);
    StandardOutWrite(arg1: null, Environment.NewLine);
  }

  public void LogDebug(string message, ConsoleColor? color = null)
  {
    StandardOutWriteAsLine(
      new[]
      {
        ((ConsoleColor?)(color ?? ConsoleColor.Magenta), message)
      }
    );
  }
}
