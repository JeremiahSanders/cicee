using System;
using System.Collections.Generic;
using System.IO;

using Cicee.CiEnv;
using Cicee.Commands.Exec;
using Cicee.Dependencies;

using LanguageExt.Common;

using Xunit;

namespace Cicee.Tests.Unit.Commands.Exec.ExecHandlingTests;

public class TryCreateRequestContext
{
  public static IEnumerable<object[]> GenerateTestCases()
  {
    string defaultProjectRoot = "/code";
    string defaultProjectName = $"name-{Guid.NewGuid():D}";
    string defaultVersion = $"0.0.0-sha-{Guid.NewGuid().ToString(format: "N").Substring(startIndex: 0, length: 7)}";
    string defaultTitle = $"Title {Guid.NewGuid():D}";
    ProjectMetadata defaultProjectMetadata = new()
    {
      Name = defaultProjectName,
      Version = defaultVersion,
      Title = defaultTitle,
      CiEnvironment = new ProjectContinuousIntegrationEnvironmentDefinition
      {
        Variables = new[]
        {
          new ProjectEnvironmentVariable
          {
            Name = $"VARIABLE_{Guid.NewGuid().ToString(format: "D").Replace(oldValue: "-", newValue: "_")}",
            DefaultValue = Randomization.Boolean() ? string.Empty : Guid.NewGuid().ToString(),
            Description = $"Description {Guid.NewGuid():D}",
            Required = Randomization.Boolean(),
            Secret = Randomization.Boolean()
          }
        }
      }
    };

    Func<string, string, string> combinePath = (path1, path2) => $"{path1}/{path2}";
    CommandDependencies baseDependencies = DependencyHelper.CreateMockDependencies() with
    {
      CombinePath = combinePath,
      DoesFileExist = file =>
      {
        string projectMetadataPath = combinePath(defaultProjectRoot, arg2: ".project-metadata.json");
        string ciDockerfilePath = combinePath(defaultProjectRoot, combinePath(arg1: "ci", arg2: "Dockerfile"));
        return file == projectMetadataPath || file == ciDockerfilePath;
      },
      TryLoadFileString = file =>
      {
        string projectMetadataPath = combinePath(defaultProjectRoot, arg2: ".project-metadata.json");
        return file == projectMetadataPath
          ? Json.TrySerialize(defaultProjectMetadata)
          : new Result<string>(new FileNotFoundException(file));
      }
    };
    ExecRequest baseRequest = new(defaultProjectRoot, Command: "-al", Entrypoint: "ls", Image: null);
    ExecRequestContext baseResult = new(
      baseRequest.ProjectRoot,
      defaultProjectMetadata,
      baseRequest.Command,
      baseRequest.Entrypoint,
      combinePath(baseRequest.ProjectRoot, combinePath(arg1: "ci", arg2: "Dockerfile")),
      Image: null
    );

    CommandDependencies happyPathDependencies = baseDependencies;
    ExecRequest happyPathRequest = baseRequest;
    Result<ExecRequestContext> happyPathResult = new(baseResult);

    return new[]
    {
      TestCase(happyPathDependencies, happyPathRequest, happyPathResult)
    };

    object[] TestCase(CommandDependencies dependencies, ExecRequest request, Result<ExecRequestContext> expected)
    {
      return new object[]
      {
        dependencies,
        request,
        expected
      };
    }
  }

  [Theory]
  [MemberData(nameof(GenerateTestCases))]
  public void ReturnsExpectedProcessStartInfo(CommandDependencies dependencies, ExecRequest execRequest,
    Result<ExecRequestContext> expectedResult)
  {
    Result<ExecRequestContext> actualResult = ExecHandling.TryCreateRequestContext(dependencies, execRequest);

    Assertions.Results.Equal(expectedResult, actualResult);
  }
}
