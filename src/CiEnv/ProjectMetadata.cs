namespace Cicee.CiEnv
{
  public record ProjectMetadata
  {
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public ProjectContinuousIntegrationEnvironmentDefinition CiEnvironment { get; init; } = new();
  }
}
