namespace Cicee.CiEnv;

public record ProjectMetadata
{
  /// <summary>
  ///   Gets the project name.
  /// </summary>
  /// <remarks>
  ///   This is expected to be a path-safe token. I.e., it could be interpolated into a file path without escaping. Value is
  ///   recommended to be in lower kebab case, e.g., <c>example-service</c>.
  /// </remarks>
  public string Name { get; init; } = string.Empty;

  /// <summary>
  ///   Gets the project description.
  /// </summary>
  /// <remarks>This is a human-readable description of the project.</remarks>
  public string Description { get; init; } = string.Empty;

  /// <summary>
  ///   Gets the project display title.
  /// </summary>
  /// <remarks>This is expected to be a human-readable display title, e.g., <c>Example Service</c>.</remarks>
  public string Title { get; init; } = string.Empty;

  /// <summary>
  ///   Gets the project version.
  /// </summary>
  /// <remarks>This value is expected to be in release SemVer format, i.e., <c>Major.Minor.Patch</c>, e.g., <c>5.1.3</c>.</remarks>
  public string Version { get; init; } = string.Empty;

  /// <summary>
  ///   Gets the CI environment definition.
  /// </summary>
  public ProjectContinuousIntegrationEnvironmentDefinition CiEnvironment { get; init; } = new();
}
