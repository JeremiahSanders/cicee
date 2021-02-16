using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace Cicee.Commands.Exec
{
  public static class ProcessHelpers
  {
    public static Task<Result<ProcessExecResult>> ExecuteProcessAsync(
      ProcessStartInfo processStartInfo,
      Action<string>? debugLogger = null
    )
    {
      return Prelude.TryAsync(async () =>
      {
        debugLogger?.Invoke(
          obj: $"Starting process.\n  Filename: {processStartInfo.FileName}\n  Arguments: {processStartInfo.Arguments}");
        var process = Process.Start(processStartInfo);
        if (process != null)
        {
          debugLogger?.Invoke(obj: $"Process filename {processStartInfo.FileName} started with id {process.Id}.");
          await process.WaitForExitAsync();
          debugLogger?.Invoke(obj: $"Process {process.Id} exited with exit code {process.ExitCode}.");
          if (process.ExitCode != 0)
          {
            throw new ExecutionException(
              message: $"{processStartInfo.FileName} returned non-zero exit code: {process.ExitCode}",
              process.ExitCode
            );
          }

          return new ProcessExecResult {ExitCode = process.ExitCode};
        }

        throw new Exception(message: $"Failed to start process {processStartInfo.FileName}");
      }).Try();
    }

    public static Result<ProcessStartInfo> TryCreateBashProcessStartInfo(
      IReadOnlyDictionary<string, string> environment,
      string arguments
    )
    {
      const string fileName = "bash";

      string CreateProcessArguments()
      {
        var environmentVariableAssignments = string.Join(
          separator: ' ',
          values: environment
            .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
            .Select(kvp => $"{kvp.Key}=\\\"{kvp.Value}\\\"")
        );

        return $"-c \"{environmentVariableAssignments} {arguments}\"";
      }

      return new Result<string>(value: CreateProcessArguments())
        .Bind(ValidateArgumentsLength)
        .Map(validatedProcessArguments =>
        {
          var startInfo = new ProcessStartInfo(fileName, validatedProcessArguments);
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
            e: new Exception(
              message:
              $"Requested execution cannot be completed. Maximum process argument length is {inclusiveMaxArgumentsLength + 1}. Actual process argument length is {processArguments.Length}."
            )
          )
          : new Result<string>(processArguments);
    }
  }
}
