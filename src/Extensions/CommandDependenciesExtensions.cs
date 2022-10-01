using System.Diagnostics;
using System.Threading.Tasks;
using Cicee.CiEnv;
using Cicee.Dependencies;
using LanguageExt.Common;

namespace Cicee.Extensions;

public static class CommandDependenciesExtensions
{
  public static async Task<Result<(ProcessExecResult ManifestInstallResult, ProcessExecResult CiceeInstallResult)>>
    TryAddCiceeLocalToolAsync(
      this CommandDependencies dependencies,
      string projectRoot
    )
  {
    return await (await dependencies
        .ProcessExecutor(
          new ProcessStartInfo("dotnet") {WorkingDirectory = projectRoot, ArgumentList = {"new", "tool-manifest"}}
        )
      )
      .BindAsync<ProcessExecResult, (ProcessExecResult, ProcessExecResult)>(
        async manifestInstallResult => (await dependencies.ProcessExecutor(
            new ProcessStartInfo("dotnet")
            {
              WorkingDirectory = projectRoot, ArgumentList = {"tool", "install", "--local", "cicee"}
            }))
          .Map(ciceeInstallResult => (toolInstallResult: manifestInstallResult, ciceeInstallResult))
      );
  }

  public static async Task<Result<string>> TryWriteMetadataFile(
    this CommandDependencies dependencies,
    string metadataFile,
    ProjectMetadata metadata
  )
  {
    return metadataFile.ToLowerInvariant().Contains("package.json")
      // Don't update package.json files.
      ? new Result<string>(metadataFile)
      : (await Json.TrySerialize(metadata)
        .BindAsync(json => dependencies.TryWriteFileStringAsync((FileName: metadataFile, Content: json)))
      )
      .Map(result => result.FileName);
  }
}
