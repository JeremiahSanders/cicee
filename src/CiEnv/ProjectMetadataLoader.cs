using System;
using System.IO;
using Cicee.Commands;
using LanguageExt.Common;

namespace Cicee.CiEnv
{
  public static class ProjectMetadataLoader
  {
    public static Result<ProjectMetadata> TryLoad(
      Func<string, Result<string>> ensureDirectoryExists,
      Func<string, Result<string>> ensureFileExists,
      Func<string, Result<string>> tryLoadFileString,
      string projectRoot
    )
    {
      const string metadataName = ".project-metadata.json";
      return ensureDirectoryExists(projectRoot)
        .Bind(validatedRoot => ensureFileExists(arg: Path.Combine(validatedRoot, metadataName)))
        .Bind(validatedFile => tryLoadFileString(validatedFile).MapLeft(loadingFailure =>
          new BadRequestException(message: "Failed to load project metadata.", loadingFailure)))
        .Bind(content => Json.TryDeserialize<ProjectMetadata>(content).MapLeft(deserializationFailure =>
          new BadRequestException(message: "Failed to deserialize project metadata.", deserializationFailure)));
    }
  }
}
