namespace Cicee.CiEnv;

public record ProjectEnvironmentVariable
{
  /// <summary>
  ///   Gets the environment variable name.
  /// </summary>
  /// <remarks>
  ///   This is expected to be formatted as expected for the CI environment. I.e., in a Bash script-based environment,
  ///   the values are in screaming snake case, e.g., <c>MY_VARIABLE</c>; in a PowerShell-based environment, the values are
  ///   Pascal cased, e.g., <c>MyVariable</c>.
  /// </remarks>
  public string Name { get; init; } = string.Empty;

  /// <summary>
  ///   Gets the human-readable description of the environment variable.
  /// </summary>
  /// <remarks>
  ///   This supplements the often-opaque <see cref="Name" /> value. E.g., a <c>RELEASE_ENVIRONMENT</c> variable may
  ///   have the description
  ///   <c>
  ///     Boolean indicating whether this environment should produce release version, rather than prerelease version,
  ///     artifacts.
  ///   </c>
  /// </remarks>
  public string Description { get; init; } = string.Empty;

  /// <summary>
  ///   Gets a value indicating whether the environment variable is required for CI workflow execution. This is understood to
  ///   be non-null and non-empty.
  /// </summary>
  public bool Required { get; init; }

  /// <summary>
  ///   Gets a value indicating whether the environment variable's value is a secret, e.g., it should not be committed to
  ///   source control and should be obfuscated in logs.
  /// </summary>
  public bool Secret { get; init; }

  /// <summary>
  ///   Gets an optional default value for the environment variable.
  /// </summary>
  public string? DefaultValue { get; init; }
}
