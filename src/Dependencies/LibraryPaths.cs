namespace Cicee.Dependencies
{
  public static class LibraryPaths
  {
    public static string Bash(CommandDependencies dependencies)
    {
      return dependencies.CombinePath(
        dependencies.CombinePath(dependencies.GetLibraryRootPath(), "ci"),
        "bash"
      );
    }

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
}
