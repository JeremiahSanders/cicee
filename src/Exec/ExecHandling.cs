using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace Cicee.Exec
{
  internal static class ExecHandling
  {
    private const string ProjectRoot = "PROJECT_ROOT";
    private const string LibRoot = "LIB_ROOT";
    private const string CiCommand = "CI_COMMAND";
    private const string CiEntrypoint = "CI_ENTRYPOINT";

    public static Result<ExecContext> ValidateEnvironment(ExecContext execContext)
    {
      var knownEnvironment = EnvironmentVariableHelpers.GetEnvironmentVariables();
      string[] knownVariables = knownEnvironment.Keys.ToArray();
      string[] missingVariables = execContext.ProjectMetadata.CiEnvironment.Variables
        .Where(envVariable => envVariable.Required && !knownVariables.Contains(envVariable.Name))
        .Select(envVariable => envVariable.Name)
        .OrderBy(Prelude.identity)
        .ToArray();

      return missingVariables.Any()
        ? new Result<ExecContext>(
          new BadRequestException(
            $"Missing environment variables: {string.Join(", ", missingVariables)}"
          )
        )
        : new Result<ExecContext>(execContext);
    }

    public static Result<ProjectMetadata> TryGetProjectMetadata(
      Func<string, Result<string>> ensureDirectoryExists,
      Func<string, Result<string>> ensureFileExists,
      Func<string, Result<string>> tryLoadFileString,
      string projectRoot)
    {
      const string metadataName = ".project-metadata.json";
      return ensureDirectoryExists(projectRoot)
        .Bind(validatedRoot => ensureFileExists(Path.Combine(validatedRoot, metadataName)))
        .Bind(validatedFile => tryLoadFileString(validatedFile).MapLeft(loadingFailure =>
          new BadRequestException("Failed to load project metadata.", loadingFailure)))
        .Bind(content => Json.TryDeserialize<ProjectMetadata>(content).MapLeft(deserializationFailure =>
          new BadRequestException("Failed to deserialize project metadata.", deserializationFailure)));
    }

    private static Result<ExecContext> ValidateContext(ExecContext execContext)
    {
      return (string.IsNullOrWhiteSpace(execContext.Command) && string.IsNullOrWhiteSpace(execContext.Entrypoint))
        ? new Result<ExecContext>(new BadRequestException("At least one of command or entrypoint must be provided."))
        : new(execContext);
    }

    private static IDictionary<string, string> GetEnvironmentDisplay(ExecContext execContext)
    {
      const string secretString = "********";
      var knownEnvironment = EnvironmentVariableHelpers.GetEnvironmentVariables();
      var expectedEnvironment = execContext.ProjectMetadata.CiEnvironment.Variables.Select(env => env.Name).ToArray();
      return new Dictionary<string, string>(
        knownEnvironment
          .Where(keyValuePair => expectedEnvironment.Contains(keyValuePair.Key))
          .Select(keyValuePair =>
            execContext.ProjectMetadata.CiEnvironment.Variables.First(envVar => envVar.Name == keyValuePair.Key).Secret
              ? new KeyValuePair<string, string>(keyValuePair.Key, secretString)
              : keyValuePair)
      );
    }

    public static Result<ProcessStartInfo> CreateProcessStartInfo(ExecContext execContext)
    {
      static string WindowsToLinuxPath(string path)
      {
        var driveAndPath = path.Split(":\\");
        return $"/{driveAndPath[0].ToLowerInvariant()}/{driveAndPath[1].Replace(oldChar: '\\', newChar: '/')}";
      }

      static string NormalizeToLinuxPath(string path)
      {
        return path.Contains(":\\") ? WindowsToLinuxPath(path) : path;
      }

      return Prelude.Try(() =>
      {
        var executionPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
        var libPath = Path.Combine(executionPath, "lib");
        var ciceeExecPath = Path.Combine(libPath, "cicee-exec.sh");
        if (!File.Exists(ciceeExecPath))
        {
          throw new Exception($"Failed to find library file: {ciceeExecPath}");
        }

        // TODO: See if the path can be inferred correctly, rather than hacking it into Linux assumptions.
        var ciceeExecLinuxPath = NormalizeToLinuxPath(ciceeExecPath);
        var startInfo = new ProcessStartInfo(
          fileName: "bash",
          arguments: $"-c \"{ciceeExecLinuxPath} {NormalizeToLinuxPath(libPath)} {NormalizeToLinuxPath(execContext.ProjectRoot)} {execContext.Command} {execContext.Entrypoint}\""
        );
        startInfo.Environment[CiCommand] = execContext.Command;
        startInfo.Environment[CiEntrypoint] = execContext.Entrypoint;
        startInfo.Environment[ProjectRoot] = NormalizeToLinuxPath(execContext.ProjectRoot);
        startInfo.Environment[LibRoot] = NormalizeToLinuxPath(libPath);
        return startInfo;
      }).Try();
    }

    private static Result<Process> ExecuteProcess(ProcessStartInfo processStartInfo)
    {
      return Prelude.Try(() =>
      {
        var process = Process.Start(processStartInfo);
        if (process != null)
        {
          process.WaitForExit();
          if (process.ExitCode != 0)
          {
            throw new ExecutionException(
              $"{processStartInfo.FileName} returned non-zero exit code: {process.ExitCode}",
              process.ExitCode);
          }

          return process;
        }

        throw new Exception($"Failed to start process {processStartInfo.FileName}");
      }).Try();
    }

    private static Result<ExecContext> TryExecute(ExecContext execContext)
    {
      return CreateProcessStartInfo(execContext).Bind(ExecuteProcess).Map(_ => execContext);
    }

    public static Task HandleAsync(string projectRoot, string? command, string? entrypoint)
    {
      Console.WriteLine($"Project root: {projectRoot}");
      Console.WriteLine($"Command: {command}");

      TryGetProjectMetadata(Io.EnsureDirectoryExists, Io.EnsureFileExists, Io.TryLoadFileString, projectRoot)
        .Map(projectMetadata => new ExecContext(projectRoot, projectMetadata, command, entrypoint))
        .Bind(ValidateContext)
        .Bind(ValidateEnvironment)
        .Map(DisplayExecContext)
        .Bind(TryExecute)
        .IfFail(exception =>
        {
          switch (exception)
          {
            case BadRequestException badRequestException:
              Console.WriteLine($"Execution failed!\nReason: {badRequestException.Message}");
              break;
            case ExecutionException executionException:
              Environment.ExitCode = executionException.ExitCode;
              Console.WriteLine($"Execution failed!\nReason: {executionException.Message}");
              break;
            default:
              Environment.ExitCode = 1;
              Console.WriteLine($"Execution failed!\nReason: {exception.Message}");
              break;
          }
        });

      return Task.CompletedTask;
    }

    private static ExecContext DisplayExecContext(ExecContext context)
    {
      Console.WriteLine("Project metadata loaded.");
      Console.WriteLine("Environment:");
      var environmentDisplay = GetEnvironmentDisplay(context);
      if (environmentDisplay.Any())
      {
        foreach (var (key, value) in environmentDisplay)
        {
          Console.WriteLine($"  {key}: {value}");
        }
      }
      else
      {
        Console.WriteLine("  No environment defined.");
      }

      return context;
    }
  }
}
