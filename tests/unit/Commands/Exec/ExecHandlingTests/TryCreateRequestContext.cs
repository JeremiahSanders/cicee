using System;
using System.Collections.Generic;
using System.IO;

using Cicee.CiEnv;
using Cicee.Commands.Exec;
using Cicee.Commands.Exec.Handling;
using Cicee.Dependencies;

using LanguageExt.Common;

using Xunit;

namespace Cicee.Tests.Unit.Commands.Exec.ExecHandlingTests;

public class TryCreateRequestContext
{
  public static IEnumerable<object[]> GenerateTestCases()
  {
    const string defaultProjectRoot = "/in-memory-test-case";
    string defaultProjectName = $"name-{Guid.NewGuid():D}";
    string defaultVersion = $"0.0.0-sha-{Guid.NewGuid().ToString(format: "N")[..7]}";
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
            DefaultValue = Randomization.Boolean()
              ? string.Empty
              : Guid
                .NewGuid()
                .ToString(),
            Description = $"Description {Guid.NewGuid():D}",
            Required = Randomization.Boolean(),
            Secret = Randomization.Boolean()
          }
        }
      }
    };

    string defaultProjectMetadataPath = CombinePath(defaultProjectRoot, path2: ".project-metadata.json");
    string defaultCiDockerfilePath = CombinePath(defaultProjectRoot, CombinePath(path1: "ci", path2: "Dockerfile"));
    string defaultCiDockerComposeProject = CombinePath(
      defaultProjectRoot,
      CombinePath(path1: "ci", path2: "docker-compose.project.yml")
    );
    string defaultCiDockerComposeDependencies = CombinePath(
      defaultProjectRoot,
      CombinePath(path1: "ci", path2: "docker-compose.dependencies.yml")
    );

    CommandDependencies baseDependencies = DependencyHelper.CreateMockDependencies() with
    {
      CombinePath = CombinePath,
      DoesFileExist =
      filePath =>
        filePath == defaultProjectMetadataPath ||
        filePath == defaultCiDockerfilePath ||
        filePath == defaultCiDockerComposeProject ||
        filePath == defaultCiDockerComposeDependencies,
      TryLoadFileString = file =>
      {
        string projectMetadataPath = CombinePath(defaultProjectRoot, path2: ".project-metadata.json");

        return file == projectMetadataPath
          ? Json.TrySerialize(defaultProjectMetadata)
          : new Result<string>(new FileNotFoundException(file));
      }
    };
    ExecRequest baseRequest = new(
      defaultProjectRoot,
      Command: "-al",
      Entrypoint: "ls",
      Image: null,
      ExecInvocationHarness.Script,
      ExecVerbosity.Normal
    );
    ExecRequestContext baseResult = new(
      baseRequest.ProjectRoot,
      defaultProjectMetadata,
      baseRequest.Command,
      baseRequest.Entrypoint,
      CombinePath(baseRequest.ProjectRoot, CombinePath(path1: "ci", path2: "Dockerfile")),
      Image: null,
      ExecInvocationHarness.Script,
      ExecVerbosity.Normal,
      CombinePath(baseRequest.ProjectRoot, path2: "ci"),
      new[]
      {
        CombinePath(baseRequest.ProjectRoot, CombinePath(path1: "ci", path2: "docker-compose.project.yml")),
        CombinePath(baseRequest.ProjectRoot, CombinePath(path1: "ci", path2: "docker-compose.dependencies.yml")),
        CombinePath(baseDependencies.GetLibraryRootPath(), path2: "docker-compose.yml"),
        CombinePath(baseDependencies.GetLibraryRootPath(), path2: "docker-compose.dockerfile.yml"),
        CombinePath(baseRequest.ProjectRoot, CombinePath(path1: "ci", path2: "docker-compose.project.yml"))
      },
      baseDependencies.CombinePath(baseRequest.ProjectRoot, baseDependencies.CombinePath(arg1: "ci", arg2: "lib")),
      IoContext.CreateCiDockerfileImageTag(defaultProjectMetadata.Name)
    );

    CommandDependencies happyPathDependencies = baseDependencies;
    ExecRequest happyPathRequest = baseRequest;
    Result<ExecRequestContext> happyPathResult = new(baseResult);

    return new[]
    {
      TestCase(happyPathDependencies, happyPathRequest, happyPathResult)
    };

    string CombinePath(string path1, string path2)
    {
      return $"{path1}/{path2}";
    }

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
  public void ReturnsExpectedProcessStartInfo(
    CommandDependencies dependencies,
    ExecRequest execRequest,
    Result<ExecRequestContext> expectedResult)
  {
    Result<ExecRequestContext> actualResult = IoContext.TryCreateRequestContext(dependencies, execRequest);

    Assertions.Results.Equal(expectedResult, actualResult);
  }
}
