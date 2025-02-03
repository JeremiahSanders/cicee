using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using Cicee.CiEnv;
using Cicee.Commands.Exec;
using Cicee.Dependencies;

using LanguageExt;
using LanguageExt.Common;

namespace Cicee.Commands.Meta.Version.Bump;

public static class MetaVersionBumpHandling
{
  public static System.Version BumpVersion(this System.Version version, SemVerIncrement semVerIncrement)
  {
    return semVerIncrement switch
    {
      SemVerIncrement.Major => new System.Version(version.Major + 1, minor: 0, build: 0),
      SemVerIncrement.Minor => new System.Version(version.Major, version.Minor + 1, build: 0),
      SemVerIncrement.Patch => new System.Version(version.Major, version.Minor, version.Build + 1),
      _ => new System.Version(version.Major, version.Major, version.Build)
    };
  }

  internal static string GetVersionString(this System.Version version)
  {
    return version.ToString(fieldCount: 3);
  }

  public static Result<(System.Version BumpedVersion, ProjectMetadata ProjectMetadata, JsonObject MetadataJson)>
    TryBumpProjectVersion(
      Func<string, Result<string>> tryLoadFileString,
      Func<string, Result<string>> ensureFileExists,
      string projectMetadataPath,
      SemVerIncrement semVerIncrement)
  {
    return ProjectMetadataLoader
      .TryLoadFromFile(ensureFileExists, tryLoadFileString, projectMetadataPath)
      .Bind(
        metadata => Prelude
          .Try(() => new System.Version(metadata.Version))
          .Try()
          .MapFailure(
            exception => new ExecutionException(
              $"Could not parse version '{metadata.Version}'.",
              exitCode: 1,
              exception
            )
          )
          .Map(version => version.BumpVersion(semVerIncrement))
          .Bind(
            bumpedVersion => TryIncrementVersionInProjectMetadata(bumpedVersion)
              .Map(
                jsonObject => (bumpedVersion, metadata with
                {
                  Version = bumpedVersion.GetVersionString()
                }, jsonObject)
              )
          )
      );

    Result<JsonObject> TryIncrementVersionInProjectMetadata(System.Version incrementedVersion)
    {
      return tryLoadFileString(projectMetadataPath)
        .MapSafe(
          content =>
          {
            JsonObject jsonObject = JsonNode.Parse(content)!.AsObject();
            jsonObject[propertyName: "version"] = incrementedVersion.GetVersionString();

            return jsonObject;
          }
        );
    }
  }

  public static Task<Result<string>> Handle(
    ICommandDependencies dependencies,
    string projectMetadataPath,
    bool isDryRun,
    SemVerIncrement semVerIncrement)
  {
    return TryBumpProjectVersion(
        dependencies.TryLoadFileString,
        dependencies.EnsureFileExists,
        projectMetadataPath,
        semVerIncrement
      )
      .BindAsync(ConditionallyModifyProjectMetadata);

    async Task<Result<string>> ConditionallyModifyProjectMetadata(
      (System.Version BumpedVersion, ProjectMetadata ProjectMetadata, JsonObject MetadataJson) tuple)
    {
      string versionString = tuple.BumpedVersion.GetVersionString();

      return isDryRun
        ? new Result<string>(versionString)
        : (await ProjectMetadataManipulation.UpdateVersionInMetadata(
          dependencies,
          projectMetadataPath,
          tuple.BumpedVersion
        )).Map(_ => versionString);
    }
  }
}
