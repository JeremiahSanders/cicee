using System;
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
    string GetDotnetToolManifestPath()
    {
      return dependencies.CombinePath(
        dependencies.CombinePath(projectRoot, ".config"),
        "dotnet-tools.json"
      );
    }

    Result<bool> IsCiceeInstalled()
    {
      return dependencies.TryLoadFileString(GetDotnetToolManifestPath())
        .Map(content => content.Contains("cicee", StringComparison.InvariantCultureIgnoreCase));
    }

    return await (await dependencies.DoesFileExist(GetDotnetToolManifestPath())
        .BindAsync(async manifestExists => manifestExists
          ? new ProcessExecResult()
          : await dependencies
            .ProcessExecutor(
              new ProcessStartInfo("dotnet") {WorkingDirectory = projectRoot, ArgumentList = {"new", "tool-manifest"}}
            )
        )
      )
      .BindAsync(manifestInstallResult =>
        IsCiceeInstalled().BindAsync<bool, (ProcessExecResult, ProcessExecResult)>(async isCiceeInstalled =>
          (await dependencies.ProcessExecutor(
            new ProcessStartInfo("dotnet")
            {
              WorkingDirectory = projectRoot,
              ArgumentList = {"tool", isCiceeInstalled ? "update" : "install", "--local", "cicee"}
            }))
          .Map(ciceeInstallResult => (toolInstallResult: manifestInstallResult, ciceeInstallResult))
        )
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
