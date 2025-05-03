using System;
using System.CommandLine;

using Cicee.CiEnv;
using Cicee.Dependencies;

namespace Cicee.Commands;

internal static class ProjectRootOption
{
  private const string Description = "Project repository root directory";

  private static string[] CreateAliases()
  {
    return new[]
    {
      "--project-root",
      "-p"
    };
  }

  public static Option<string> Of(Func<string> getDefaultValue)
  {
    return new Option<string>(CreateAliases(), getDefaultValue, Description)
    {
      IsRequired = true
    };
  }


  public static Option<string> Create(ICommandDependencies dependencies)
  {
    return Of(() => InferProjectRoot(dependencies));
  }

  public static Option<string?> CreateOptional(ICommandDependencies dependencies)
  {
    return new Option<string?>(CreateAliases(), () => InferProjectRoot(dependencies), Description)
    {
      IsRequired = false
    };
  }

  private static string InferProjectRoot(ICommandDependencies dependencies)
  {
    return ProjectMetadataLoader
      .InferProjectMetadataPath(
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
