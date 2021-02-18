using System;
using System.Collections.Generic;
using System.IO;
using Cicee.CiEnv;
using Cicee.Commands.Exec;
using LanguageExt.Common;
using Xunit;

namespace Cicee.Tests.Unit.Commands.Exec.ExecHandlingTests
{
  public class TryCreateRequestContext
  {
    public static IEnumerable<object[]> GenerateTestCases()
    {
      object[] TestCase(ExecDependencies dependencies, ExecRequest request, Result<ExecRequestContext> expected)
      {
        return new object[] {dependencies, request, expected};
      }

      var defaultProjectRoot = "/code";
      var defaultProjectName = $"name-{Guid.NewGuid():D}";
      var defaultVersion = $"0.0.0-sha-{Guid.NewGuid().ToString(format: "N").Substring(startIndex: 0, length: 7)}";
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
                  $"VARIABLE_{Guid.NewGuid().ToString(format: "D").Replace(oldValue: "-", newValue: "_")}",
                DefaultValue = Randomization.Boolean() ? string.Empty : Guid.NewGuid().ToString(),
                Description = $"Description {Guid.NewGuid():D}",
                Required = Randomization.Boolean(),
                Secret = Randomization.Boolean()
              }
            }
          }
        };

      var baseDependencies = ExecHandlingTestHelpers.CreateDependencies() with
      {
        EnsureFileExists = file =>
        {
          var ciEnvPath = Path.Combine(defaultProjectRoot, path2: "ci", path3: "ci.env");
          var projectMetadataPath = Path.Combine(defaultProjectRoot, path2: ".project-metadata.json");
          var ciDockerfilePath = Path.Combine(defaultProjectRoot, "ci", "Dockerfile");
          return file == ciEnvPath || file == projectMetadataPath || file == ciDockerfilePath
            ? new Result<string>(file)
            : new Result<string>(e: new FileNotFoundException(file));
        },
        TryLoadFileString = file =>
        {
          var projectMetadataPath = Path.Combine(defaultProjectRoot, path2: ".project-metadata.json");
          return file == projectMetadataPath
            ? Json.TrySerialize(defaultProjectMetadata)
            : new Result<string>(e: new FileNotFoundException(file));
        }
      };
      var baseRequest = new ExecRequest(defaultProjectRoot, Command: "-al", Entrypoint: "ls",
        Image: null);
      var baseResult = new ExecRequestContext(
        baseRequest.ProjectRoot,
        defaultProjectMetadata,
        baseRequest.Command,
        baseRequest.Entrypoint,
        EnvironmentInitializationScriptPath: Path.Combine(baseRequest.ProjectRoot, path2: "ci", path3: "ci.env"),
        Dockerfile: Path.Combine(baseRequest.ProjectRoot, path2: "ci", path3: "Dockerfile"),
        Image: null
      );

      var happyPathDependencies = baseDependencies;
      var happyPathRequest = baseRequest;
      var happyPathResult = new Result<ExecRequestContext>(baseResult);

      return new[] {TestCase(happyPathDependencies, happyPathRequest, happyPathResult)};
    }

    [Theory]
    [MemberData(memberName: nameof(GenerateTestCases))]
    public void ReturnsExpectedProcessStartInfo(
      ExecDependencies dependencies,
      ExecRequest execRequest,
      Result<ExecRequestContext> expectedResult
    )
    {
      var actualResult = ExecHandling.TryCreateRequestContext(dependencies, execRequest);

      expectedResult.IfSucc(expected =>
      {
        actualResult.IfSucc(actual =>
        {
          Assert.Equal(expected, actual);
        });
        actualResult.IfFail(actualException =>
        {
          throw actualException;
        });
      });
      expectedResult.IfFail(exception =>
      {
        actualResult.IfSucc(actual =>
        {
          throw new Exception(message: "Should have failed");
        });
        actualResult.IfFail(actual =>
        {
          Assert.Equal(expected: exception.GetType(), actual: actual.GetType());
          Assert.Equal(exception.Message, actual.Message);
        });
      });
    }
  }
}
