using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cicee.CiEnv;
using Cicee.Commands;
using Cicee.Commands.Exec;
using LanguageExt.Common;
using Xunit;

namespace Cicee.Tests.Unit.Commands.Exec.ExecHandlingTests
{
  public class HandleAsync
  {
    public static IEnumerable<object[]> GenerateTestCases()
    {
      object[] TestCase(CommandDependencies dependencies, ExecRequest request, Result<ExecResult> expected)
      {
        return new object[] {dependencies, request, expected};
      }

      var defaultLibraryRoot = "/cicee/lib";
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
        DoesFileExist = file =>
        {
          var ciEnvPath = combinePath(defaultProjectRoot, combinePath("ci", "ci.env"));
          var projectMetadataPath = combinePath(defaultProjectRoot, ".project-metadata.json");
          var ciceeExecPath = combinePath(defaultLibraryRoot, "cicee-exec.sh");
          var ciDockerfilePath = combinePath(defaultProjectRoot, combinePath("ci", "Dockerfile"));
          return file == ciEnvPath || file == projectMetadataPath || file == ciceeExecPath || file == ciDockerfilePath;
        },
        TryLoadFileString = file =>
        {
          var projectMetadataPath = combinePath(defaultProjectRoot, ".project-metadata.json");
          return file == projectMetadataPath
            ? Json.TrySerialize(defaultProjectMetadata)
            : new Result<string>(new FileNotFoundException(file));
        },
        GetEnvironmentVariables = () =>
          new Dictionary<string, string>(defaultProjectMetadata.CiEnvironment.Variables.Select(variable =>
              new KeyValuePair<string, string>(variable.Name, Randomization.GuidString())
            )
          ),
        GetLibraryRootPath = () => defaultLibraryRoot
      };
      var baseRequest = new ExecRequest(defaultProjectRoot, "-al", "ls", Image: null);
      var baseResult = new ExecResult(baseRequest);

      var happyPathDependencies = baseDependencies;
      var happyPathRequest = baseRequest;
      var happyPathResult = new Result<ExecResult>(baseResult);

      return new[] {TestCase(happyPathDependencies, happyPathRequest, happyPathResult)};
    }

    [Theory]
    [MemberData(nameof(GenerateTestCases))]
    public async Task ReturnsExpectedResult(
      CommandDependencies dependencies,
      ExecRequest execRequest,
      Result<ExecResult> expectedResult
    )
    {
      var actualResult = await ExecHandling.HandleAsync(dependencies, execRequest);

      Assertions.Results.Equal(expectedResult, actualResult);
    }
  }
}
