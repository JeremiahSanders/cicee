using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cicee.Commands.Exec;
using LanguageExt;
using LanguageExt.Common;

namespace Cicee.Dependencies;

public static class ProcessHelpers
{
  private const string DefaultBashFileName = "bash";

  public static string? TryFindBash()
  {
    string? path = null;

    // On Windows, 'bash' is provided by WSL. That maintains separate environment variables.
    // We need to use Git Bash, if it exists.
    var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
    if (!string.IsNullOrWhiteSpace(programFiles))
    {
      // Program Files exists; we must be on Windows.
      var expectedGitBashPath = Path.Combine(programFiles, "Git", "bin", "bash.exe");

      if (File.Exists(expectedGitBashPath))
      {
        path = expectedGitBashPath;
      }
      else
      {
        // Since we didn't find Git Bash, we have to default to WSL for bash.
        path = DefaultBashFileName;
      }
    }
    else
    {
      // We'll assume we're on *nix and use 'bash'.
      path = DefaultBashFileName;
    }

    return path;
  }

  public static Task<Result<ProcessExecResult>> ExecuteProcessAsync(
    ProcessStartInfo processStartInfo,
    Action<string>? debugLogger = null
  )
  {
    return Prelude.TryAsync(async () =>
    {
      debugLogger?.Invoke(
        $"Starting process.\n  Filename: {processStartInfo.FileName}\n  Arguments: {processStartInfo.Arguments}");
      var process = Process.Start(processStartInfo);
      if (process == null)
      {
        throw new ExecutionException($"Failed to start process {processStartInfo.FileName}", exitCode: 1);
      }

      debugLogger?.Invoke($"Process filename {processStartInfo.FileName} started with id {process.Id}.");
      await process.WaitForExitAsync();
      debugLogger?.Invoke($"Process {process.Id} exited with exit code {process.ExitCode}.");
      if (process.ExitCode != 0)
      {
        throw new ExecutionException(
          $"{processStartInfo.FileName} returned non-zero exit code: {process.ExitCode}",
          process.ExitCode
        );
      }

      return new ProcessExecResult {ExitCode = process.ExitCode};
    }).Try();
  }

  public static Result<ProcessStartInfo> TryCreateBashProcessStartInfo(
    IReadOnlyDictionary<string, string> environment,
    string arguments
  )
  {
    string CreateProcessArguments()
    {
      return $"-c \"{arguments}\"";
    }

    return new Result<string>(CreateProcessArguments())
      .Bind(ValidateArgumentsLength)
      .Map(validatedProcessArguments =>
      {
        var startInfo = new ProcessStartInfo(TryFindBash() ?? DefaultBashFileName, validatedProcessArguments);
        foreach (var keyValuePair in environment)
        {
          startInfo.Environment[keyValuePair.Key] = keyValuePair.Value;
        }

        return startInfo;
      });
  }

  private static Result<string> ValidateArgumentsLength(string processArguments)
  {
    // Maximum argument length reference: https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.arguments?view=net-5.0
    const int inclusiveMaxArgumentsLength = 32698;
    return
      processArguments.Length > inclusiveMaxArgumentsLength
        ? new Result<string>(
          new ArgumentException(
            $"Requested execution cannot be completed. Maximum process argument length is {inclusiveMaxArgumentsLength + 1}. Actual process argument length is {processArguments.Length}.",
            nameof(processArguments)
          )
        )
        : new Result<string>(processArguments);
  }
}
