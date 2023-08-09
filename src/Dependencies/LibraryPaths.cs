namespace Cicee.Dependencies;

/// <summary>
///   Derives paths within the CICEE installation's CI library.
/// </summary>
public static class LibraryPaths
{
  /// <summary>
  ///   Derives the path to <c>ci/bash</c> within the CI library.
  /// </summary>
  public static string Bash(CommandDependencies dependencies)
  {
    return dependencies.CombinePath(
      dependencies.CombinePath(dependencies.GetLibraryRootPath(), "ci"),
      "bash"
    );
  }

  /// <summary>
  ///   Derives the path to <c>ci/bash/ci.sh</c> within the CI library.
  /// </summary>
  public static string BashEntrypoint(CommandDependencies dependencies)
  {
    return dependencies.CombinePath(Bash(dependencies), "ci.sh");
  }

  public static string CiceeBash(CommandDependencies dependencies)
  {
    return dependencies.GetLibraryRootPath();
  }

  public static string DockerCompose(CommandDependencies dependencies)
  {
    return dependencies.GetLibraryRootPath();
  }
}
