using System;
using System.CommandLine;
using Cicee.CiEnv;
using Cicee.Dependencies;

namespace Cicee.Commands;

internal static class ProjectRootOption
{
  public static Option<string> Of(Func<string> getDefaultValue)
  {
    return new Option<string>(
      new[] {"--project-root", "-p"},
      getDefaultValue,
      "Project repository root directory"
    ) {IsRequired = true};
  }

  public static Option<string> Create(CommandDependencies dependencies)
  {
    return Of(() => InferProjectRoot(dependencies));
  }

  private static string InferProjectRoot(CommandDependencies dependencies)
  {
    return ProjectMetadataLoader.InferProjectMetadataPath(
        dependencies.EnsureDirectoryExists,
        dependencies.EnsureFileExists,
        dependencies.TryLoadFileString,
        dependencies.CombinePath,
        dependencies.TryGetParentDirectory,
        dependencies.TryGetCurrentDirectory
      )
      .Bind(dependencies.TryGetParentDirectory)
      .BindFailure(_ => dependencies.TryGetCurrentDirectory())
      .IfFail(string.Empty);
  }
}
