namespace Cicee.CiEnv
{
  public record ProjectEnvironmentVariable
  {
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool Required { get; init; }
    public bool Secret { get; init; }
    public string? DefaultValue { get; init; }
  }
}
