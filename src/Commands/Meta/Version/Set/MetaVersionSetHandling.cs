using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Cicee.CiEnv;
using Cicee.Dependencies;
using LanguageExt.Common;

namespace Cicee.Commands.Meta.Version.Set;

public static class MetaVersionSetHandling
{
  internal static string GetVersionString(this System.Version version)
  {
    return version.ToString(fieldCount: 3);
  }

  public static Result<(System.Version Version, ProjectMetadata ProjectMetadata, JsonObject MetadataJson)>
    TrySetProjectVersion(
      Func<string, Result<string>> tryLoadFileString,
      Func<string, Result<string>> ensureFileExists,
      string projectMetadataPath,
      System.Version version
    )
  {
    Result<JsonObject> TryIncrementVersionInProjectMetadata(System.Version incrementedVersion)
    {
      return tryLoadFileString(projectMetadataPath)
        .MapSafe(content =>
        {
          var jsonObject = JsonNode.Parse(content)!.AsObject();
          jsonObject["version"] = incrementedVersion.GetVersionString();
          return jsonObject;
        });
    }

    return ProjectMetadataLoader.TryLoadFromFile(
        ensureFileExists,
        tryLoadFileString,
        projectMetadataPath
      )
      .Bind(metadata =>
        TryIncrementVersionInProjectMetadata(version)
          .Map(jsonObject =>
            (version, metadata with {Version = version.GetVersionString()}, jsonObject)
          )
      );
  }

  public static Task<Result<string>> Handle(
    CommandDependencies dependencies,
    string projectMetadataPath,
    bool isDryRun,
    System.Version version
  )
  {
    async Task<Result<string>> ConditionallyModifyProjectMetadata(
      (System.Version SetedVersion, ProjectMetadata ProjectMetadata, JsonObject MetadataJson) tuple
    )
    {
      var versionString = tuple.SetedVersion.GetVersionString();
      return isDryRun
        ? new Result<string>(versionString)
        : (await ProjectMetadataManipulation.UpdateVersionInMetadata(dependencies, projectMetadataPath,
          tuple.SetedVersion)).Map(_ => versionString);
    }

    return TrySetProjectVersion(
        dependencies.TryLoadFileString,
        dependencies.EnsureFileExists,
        projectMetadataPath,
        version
      )
      .BindAsync(ConditionallyModifyProjectMetadata);
  }
}
