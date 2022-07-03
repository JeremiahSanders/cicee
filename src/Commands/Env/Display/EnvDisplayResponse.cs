using System.Collections.Generic;
using Cicee.CiEnv;

namespace Cicee.Commands.Env.Display;

public record EnvDisplayResponse
{
  public IReadOnlyDictionary<ProjectEnvironmentVariable, string> Environment { get; init; } =
    new Dictionary<ProjectEnvironmentVariable, string>();

  public string ProjectMetadataPath { get; init; } = string.Empty;
  public ProjectMetadata ProjectMetadata { get; init; } = new();
}
