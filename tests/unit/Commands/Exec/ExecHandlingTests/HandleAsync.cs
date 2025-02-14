using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Cicee.CiEnv;
using Cicee.Commands.Exec;
using Cicee.Dependencies;

using LanguageExt.Common;

using Xunit;

namespace Cicee.Tests.Unit.Commands.Exec.ExecHandlingTests;

public class HandleAsync
{
  public static IEnumerable<object[]> GenerateTestCases()
  {
    string defaultLibraryRoot = "/cicee/lib";
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

    CommandDependencies baseDependencies = DependencyHelper.CreateMockDependencies() with
    {
      DoesFileExist = file =>
      {
        string ciEnvPath = CombinePath(defaultProjectRoot, CombinePath(path1: "ci", path2: "ci.env"));
        string projectMetadataPath = CombinePath(defaultProjectRoot, path2: ".project-metadata.json");
        string ciceeExecPath = CombinePath(defaultLibraryRoot, path2: "cicee-exec.sh");
        string ciDockerfilePath = CombinePath(defaultProjectRoot, CombinePath(path1: "ci", path2: "Dockerfile"));

        return file == ciEnvPath || file == projectMetadataPath || file == ciceeExecPath || file == ciDockerfilePath;
      },
      TryLoadFileString = file =>
      {
        string projectMetadataPath = CombinePath(defaultProjectRoot, path2: ".project-metadata.json");

        return file == projectMetadataPath
          ? Json.TrySerialize(defaultProjectMetadata)
          : new Result<string>(new FileNotFoundException(file));
      },
      GetEnvironmentVariables = () => new Dictionary<string, string>(
        defaultProjectMetadata.CiEnvironment.Variables.Select(
          variable => new KeyValuePair<string, string>(variable.Name, Randomization.GuidString())
        )
      ),
      GetLibraryRootPath = () => defaultLibraryRoot
    };
    ExecRequest baseScriptHandlingRequest = new(
      defaultProjectRoot,
      Command: "-al",
      Entrypoint: "ls",
      Image: null,
      ExecInvocationHarness.Script,
      ExecVerbosity.Normal
    );
    ExecResult baseScriptHandlingResult = new(baseScriptHandlingRequest);

    ExecRequest baseDirectHandlingRequest = baseScriptHandlingRequest with
    {
      Harness = ExecInvocationHarness.Direct
    };
    ExecResult baseDirectHandlingResult = new(baseDirectHandlingRequest);

    CommandDependencies happyPathDependencies = baseDependencies;

    ExecRequest happyPathScriptHandlingRequest = baseScriptHandlingRequest;
    Result<ExecResult> happyPathScriptHandlingResult = new(baseScriptHandlingResult);

    ExecRequest happyPathDirectHandlingRequest = baseDirectHandlingRequest;
    ExecResult happyPathDirectHandlingResult = baseDirectHandlingResult;

    return new[]
    {
      TestCase(happyPathDependencies, happyPathScriptHandlingRequest, happyPathScriptHandlingResult),
      TestCase(happyPathDependencies, happyPathDirectHandlingRequest, happyPathDirectHandlingResult)
    };

    string CombinePath(string path1, string path2)
    {
      return $"{path1}/{path2}";
    }

    object[] TestCase(CommandDependencies dependencies, ExecRequest request, Result<ExecResult> expected)
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
  public async Task ReturnsExpectedResult(
    CommandDependencies dependencies,
    ExecRequest execRequest,
    Result<ExecResult> expectedResult)
  {
    Result<ExecResult> actualResult = await ExecHandler.HandleAsync(dependencies, execRequest);

    Assertions.Results.Equal(expectedResult, actualResult);
  }
}
