using System;

using Cicee.CiEnv;
using Cicee.Dependencies;
using Cicee.Tests.Unit.Commands;

using LanguageExt.Common;

using Xunit;

namespace Cicee.Tests.Unit.CiEnv.ProjectMetadataLoaderTests;

public static class InferProjectMetadataPathTests
{
  private static Result<string> Act(CommandDependencies dependencies)
  {
    return ProjectMetadataLoader.InferProjectMetadataPath(
      dependencies.EnsureDirectoryExists,
      dependencies.EnsureFileExists,
      dependencies.TryLoadFileString,
      dependencies.CombinePath,
      dependencies.TryGetParentDirectory,
      dependencies.TryGetCurrentDirectory
    );
  }

  public static class GivenExecutedInChildDirectory
  {
    public class GivenNonDefaultProjectMetadataFile
    {
      [Fact]
      public void ReturnsDetectedMetadata()
      {
        string projectMetadataFileName = "package.json";
        string projectRoot = "/home/users/fake-user";
        string currentDirectory = $"{projectRoot}/lib/important-lib";
        string expectedPath = $"{projectRoot}/{projectMetadataFileName}";
        CommandDependencies dependencies = DependencyHelper.CreateMockDependencies() with
        {
          DoesFileExist = path => path == expectedPath,
          TryGetCurrentDirectory = () => currentDirectory,
          TryLoadFileString = path => path == expectedPath
            ? new Result<string>(value: "{}")
            : new Result<string>(new Exception($"Nothing arranged for {path}"))
        };

        Result<string> result = Act(dependencies);

        Assert.Equal(new Result<string>(expectedPath), result);
      }
    }
  }
}
