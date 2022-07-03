using System;
using System.Collections.Generic;
using Cicee.CiEnv;
using LanguageExt.Common;

namespace Cicee.Commands.Env.Display;

public static class EnvDisplayHandling
{
  public static Result<EnvDisplayResponse> TryHandle(
    Func<string, Result<string>> ensureFileExists,
    Func<string, Result<string>> tryLoadFileString,
    Func<IReadOnlyDictionary<string, string>> getEnvironmentVariables,
    string projectMetadataPath
  )
  {
    return ProjectMetadataLoader.TryLoadFromFile(
      ensureFileExists,
      tryLoadFileString,
      projectMetadataPath
    ).Map(projectMetadata =>
      new EnvDisplayResponse
      {
        Environment = ProjectEnvironmentHelpers.GetEnvironmentDisplay(getEnvironmentVariables, projectMetadata),
        ProjectMetadata = projectMetadata,
        ProjectMetadataPath = projectMetadataPath
      }
    );
  }
}
