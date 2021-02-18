using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Cicee.Commands.Exec
{
  public static class ExecEntrypoint
  {
    public static async Task HandleAsync(string projectRoot, string? command, string? entrypoint, string? image)
    {
      static void SetExitCode(int exitCode)
      {
        Environment.ExitCode = exitCode;
      }

      static string GetLibraryRootPath()
      {
        var executionPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
        return Path.Combine(executionPath, path2: "lib");
      }

      var dependencies = new ExecDependencies
      (
        StandardOutWriteLine: Console.WriteLine,
        EnsureDirectoryExists: Io.EnsureDirectoryExists,
        EnsureFileExists: Io.EnsureFileExists,
        TryLoadFileString: Io.TryLoadFileString,
        ProcessExecutor: processStartInfo => ProcessHelpers.ExecuteProcessAsync(processStartInfo, debugLogger: null),
        SetExitCode: SetExitCode,
        GetEnvironmentVariables: EnvironmentVariableHelpers.GetEnvironmentVariables,
        GetLibraryRootPath: GetLibraryRootPath
      );

      (await ExecHandling.HandleAsync(
          dependencies,
          request: new ExecRequest(
            projectRoot,
            command,
            entrypoint,
            image
          )
        ))
        .IfFail(exception =>
        {
          dependencies.StandardOutWriteLine(obj: $"Execution failed!\nReason: {exception.Message}");
          switch (exception)
          {
            case ExecutionException executionException:
              dependencies.SetExitCode(executionException.ExitCode);
              break;
            default:
              dependencies.SetExitCode(obj: 1);
              break;
          }
        });
    }
  }
}
