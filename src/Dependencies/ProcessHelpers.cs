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


  public static (string BashPath, bool IsWsl) TryFindBash()
  {
    string path;
    bool isWsl;

    // On Windows, 'bash' is provided by WSL. That maintains separate environment variables.
    // We need to use Git Bash, if it exists.
    string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

    if (!string.IsNullOrWhiteSpace(programFiles) && IsEnvironmentWindows())
    {
      // Program Files is defined and OS appears to be Windows.
      string expectedGitBashPath = Path.Combine(programFiles, path2: "Git", path3: "bin", path4: "bash.exe");

      if (File.Exists(expectedGitBashPath))
      {
        path = expectedGitBashPath;
        isWsl = false;
      }
      else
      {
        // Since we didn't find Git Bash, we have to default to WSL for bash.
        path = DefaultBashFileName;
        isWsl = true;
      }
    }
    else
    {
      // We'll assume we're on *nix and use 'bash'.
      path = DefaultBashFileName;
      isWsl = false;
    }

    return (path, isWsl);

    bool IsEnvironmentWindows()
    {
      return Environment.OSVersion.Platform switch
      {
        PlatformID.Win32S => true,
        PlatformID.Win32Windows => true,
        PlatformID.Win32NT => true,
        PlatformID.WinCE => true,
        PlatformID.Unix => false,
        PlatformID.Xbox => false,
        PlatformID.MacOSX => false,
        PlatformID.Other => false,
        _ => false
      };
    }
  }

  public static Task<Result<ProcessExecResult>> ExecuteProcessAsync(
    ProcessStartInfo processStartInfo,
    Action<string>? debugLogger = null)
  {
    return Prelude
      .TryAsync(
        async () =>
        {
          debugLogger?.Invoke(
            $"Starting process.\n  Filename: {processStartInfo.FileName}\n  Arguments: {processStartInfo.Arguments}"
          );
          Process? process = Process.Start(processStartInfo);
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

          return new ProcessExecResult
          {
            ExitCode = process.ExitCode
          };
        }
      )
      .Try();
  }

  /// <summary>
  ///   Attempts to create a <see cref="ProcessStartInfo" /> for <c>bash</c>.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     When CICEE infers that Windows Subsystem for Linux (WSL) <c>bash</c> will be used,
  ///     <paramref name="requiredEnvironment" /> will be passed inline. WSL processes often maintain a separate
  ///     environment which do no inherit the ambient Windows environment variables.
  ///   </para>
  /// </remarks>
  /// <param name="requiredEnvironment">
  ///   Environment variables which are required. If any conflict with
  ///   <paramref name="ambientEnvironment" />, the required value is used.
  /// </param>
  /// <param name="ambientEnvironment">
  ///   Environment variables which should be available to the <see cref="Process" />, if
  ///   possible.
  /// </param>
  /// <param name="arguments">
  ///   Arguments which will be passed to <c>bash</c>, in the form of: <c>-c \"${ARGUMENTS}\"</c>
  /// </param>
  /// <returns></returns>
  public static Result<ProcessExecRequest> TryCreateBashProcessStartInfo(
    IReadOnlyDictionary<string, string> requiredEnvironment,
    IReadOnlyDictionary<string, string> ambientEnvironment,
    string arguments)
  {
    (string bashPath, bool isWslBash) = TryFindBash();

    return new Result<string>(CreateProcessArguments(isWslBash))
      .Bind(ValidateArgumentsLength)
      .Map(
        validatedProcessArguments =>
        {
          ProcessExecRequest startInfo = new()
          {
            FileName = bashPath, Arguments = validatedProcessArguments
          };
          foreach (KeyValuePair<string, string> keyValuePair in ambientEnvironment)
          {
            startInfo.Environment[keyValuePair.Key] = keyValuePair.Value;
          }

          foreach (KeyValuePair<string, string> keyValuePair in requiredEnvironment)
          {
            startInfo.Environment[keyValuePair.Key] = keyValuePair.Value;
          }

          return startInfo;
        }
      );

    string CreateProcessArguments(bool isWsl)
    {
      // When launching WSL bash, environment variables are not shared from Windows. We must pass required variables with the command.
      string environmentVariableAssignments = isWsl
        ? string.Join(
          separator: ' ',
          requiredEnvironment
            .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
            .Select(kvp => $"{kvp.Key}=\\\"{kvp.Value}\\\"")
        ) + " "
        : string.Empty;

      return $"-c \"{environmentVariableAssignments}{arguments}\"";
    }
  }

  private static Result<string> ValidateArgumentsLength(string processArguments)
  {
    // Maximum argument length reference: https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.arguments?view=net-5.0
    const int inclusiveMaxArgumentsLength = 32698;

    return processArguments.Length > inclusiveMaxArgumentsLength
      ? new Result<string>(
        new ArgumentException(
          $"Requested execution cannot be completed. Maximum process argument length is {inclusiveMaxArgumentsLength + 1}. Actual process argument length is {processArguments.Length}.",
          nameof(processArguments)
        )
      )
      : new Result<string>(processArguments);
  }
}
