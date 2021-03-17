using System;
using System.Collections.Generic;
using System.IO;
using Cicee.CiEnv;
using Cicee.Commands;
using Cicee.Commands.Exec;
using LanguageExt.Common;
using Xunit;

namespace Cicee.Tests.Unit.Commands.Exec.ExecHandlingTests
{
  public class TryCreateRequestContext
  {
    public static IEnumerable<object[]> GenerateTestCases()
    {
      object[] TestCase(CommandDependencies dependencies, ExecRequest request, Result<ExecRequestContext> expected)
      {
        return new object[] {dependencies, request, expected};
      }

      var defaultProjectRoot = "/code";
      var defaultProjectName = $"name-{Guid.NewGuid():D}";
      var defaultVersion = $"0.0.0-sha-{Guid.NewGuid().ToString("N").Substring(startIndex: 0, length: 7)}";
      var defaultTitle = $"Title {Guid.NewGuid():D}";
      var defaultProjectMetadata =
        new ProjectMetadata
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
                Name =
                  $"VARIABLE_{Guid.NewGuid().ToString("D").Replace("-", "_")}",
                DefaultValue = Randomization.Boolean() ? string.Empty : Guid.NewGuid().ToString(),
                Description = $"Description {Guid.NewGuid():D}",
                Required = Randomization.Boolean(),
                Secret = Randomization.Boolean()
              }
            }
          }
        };

      Func<string, string, string> combinePath = (path1, path2) => $"{path1}/{path2}";
      var baseDependencies = DependencyHelper.CreateMockDependencies() with
      {
        CombinePath = combinePath,
        EnsureFileExists = file =>
        {
          var ciEnvPath = combinePath(defaultProjectRoot, combinePath("ci", "ci.env"));
          var projectMetadataPath = combinePath(defaultProjectRoot, ".project-metadata.json");
          var ciDockerfilePath = combinePath(defaultProjectRoot, combinePath("ci", "Dockerfile"));
          return file == ciEnvPath || file == projectMetadataPath || file == ciDockerfilePath
            ? new Result<string>(file)
            : new Result<string>(new FileNotFoundException(file));
        },
        TryLoadFileString = file =>
        {
          var projectMetadataPath = combinePath(defaultProjectRoot, ".project-metadata.json");
          return file == projectMetadataPath
            ? Json.TrySerialize(defaultProjectMetadata)
            : new Result<string>(new FileNotFoundException(file));
        }
      };
      var baseRequest = new ExecRequest(defaultProjectRoot, "-al", "ls",
        Image: null);
      var baseResult = new ExecRequestContext(
        baseRequest.ProjectRoot,
        defaultProjectMetadata,
        baseRequest.Command,
        baseRequest.Entrypoint,
        combinePath(baseRequest.ProjectRoot, combinePath("ci", "ci.env")),
        combinePath(baseRequest.ProjectRoot, combinePath("ci", "Dockerfile")),
        Image: null
      );

      var happyPathDependencies = baseDependencies;
      var happyPathRequest = baseRequest;
      var happyPathResult = new Result<ExecRequestContext>(baseResult);

      return new[] {TestCase(happyPathDependencies, happyPathRequest, happyPathResult)};
    }

    [Theory]
    [MemberData(nameof(GenerateTestCases))]
    public void ReturnsExpectedProcessStartInfo(
      CommandDependencies dependencies,
      ExecRequest execRequest,
      Result<ExecRequestContext> expectedResult
    )
    {
      var actualResult = ExecHandling.TryCreateRequestContext(dependencies, execRequest);

      Assertions.Results.Equal(expectedResult, actualResult);
    }
  }
}
