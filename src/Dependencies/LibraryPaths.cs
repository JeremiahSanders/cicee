namespace Cicee.Dependencies;

/// <summary>
///   Derives paths within the CICEE installation's CI library.
/// </summary>
public static class LibraryPaths
{
  /// <summary>
  ///   Derives the path to <c>ci/bash</c> within the CI library.
  /// </summary>
  public static string Bash(ICommandDependencies dependencies)
  {
    return dependencies.CombinePath(
      dependencies.CombinePath(dependencies.GetLibraryRootPath(), suffix: "ci"),
      suffix: "bash"
    );
  }

  /// <summary>
  ///   Derives the path to <c>ci/bash/ci.sh</c> within the CI library.
  /// </summary>
  public static string BashEntrypoint(ICommandDependencies dependencies)
  {
    return dependencies.CombinePath(Bash(dependencies), suffix: "ci.sh");
  }

  public static string CiceeBash(ICommandDependencies dependencies)
  {
    return dependencies.GetLibraryRootPath();
  }

  public static string DockerCompose(ICommandDependencies dependencies)
  {
    return dependencies.GetLibraryRootPath();
  }
}
