using System;
using Cicee.CiEnv;
using Cicee.Commands;
using Cicee.Tests.Unit.Commands;
using LanguageExt.Common;
using Xunit;

namespace Cicee.Tests.Unit.CiEnv.ProjectMetadataLoaderTests;

public class InferProjectMetadataPathTests
{
  private static Result<string> Act(CommandDependencies dependencies)
  {
    return ProjectMetadataLoader.InferProjectMetadataPath(dependencies.EnsureDirectoryExists,
      dependencies.EnsureFileExists, dependencies.TryLoadFileString, dependencies.CombinePath,
      dependencies.TryGetParentDirectory, dependencies.TryGetCurrentDirectory);
  }

  public class GivenExecutedInChildDirectory
  {
    public class GivenNonDefaultProjectMetadataFile
    {
      [Fact]
      public void ReturnsDetectedMetadata()
      {
        var projectMetadataFileName = "package.json";
        var projectRoot = "/home/users/fake-user";
        var currentDirectory = $"{projectRoot}/lib/important-lib";
        var expectedPath = $"{projectRoot}/{projectMetadataFileName}";
        var dependencies = DependencyHelper.CreateMockDependencies() with
        {
          DoesFileExist = path => path == expectedPath,
          TryGetCurrentDirectory = () => currentDirectory,
          TryLoadFileString = path =>
            path == expectedPath
              ? new Result<string>("{}")
              : new Result<string>(new Exception($"Nothing arranged for {path}"))
        };

        var result = Act(dependencies);

        Assert.Equal(new Result<string>(expectedPath), result);
      }
    }
  }
}
