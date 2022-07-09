using Cicee.CiEnv;

namespace Cicee.Commands.Lib.Exec;

public record LibExecRequest
{
  /// <summary>
  ///   Gets the command which will be executed by <see cref="Shell" />.
  /// </summary>
  public string Command { get; init; } = string.Empty;

  /// <summary>
  ///   Gets the shell which will execute <see cref="Command" />.
  /// </summary>
  public LibraryShellTemplate Shell { get; init; } = LibraryShellTemplate.Bash;

  public ProjectMetadata Metadata { get; init; } = new();
  public string MetadataPath { get; init; } = string.Empty;
  public string ProjectRoot { get; init; } = string.Empty;
}
