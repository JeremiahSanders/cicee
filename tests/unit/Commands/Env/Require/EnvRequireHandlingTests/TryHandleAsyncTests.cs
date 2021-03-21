using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cicee.CiEnv;
using Cicee.Commands;
using Cicee.Commands.Env.Require;
using LanguageExt.Common;
using Xunit;

namespace Cicee.Tests.Unit.Commands.Env.Require.EnvRequireHandlingTests
{
  public class TryHandleAsyncTests
  {
    public static IEnumerable<object[]> GenerateTestCases()
    {
      // NOTE: Parameter order affects displayed test case name in IDE test explorers. Placing request first helps identification.
      object[] TestCase(EnvRequireRequest request, Result<EnvRequireResult> expected,
        CommandDependencies commandDependencies)
      {
        return new object[] {request, expected, commandDependencies};
      }

      static ProjectEnvironmentVariable GenerateRandomEnvironmentVariable()
      {
        return new()
        {
          Description = Randomization.GuidString(),
          Name = Randomization.GuidString(),
          Required = Randomization.Boolean(),
          Secret = Randomization.Boolean(),
          DefaultValue = Randomization.Boolean() ? Randomization.GuidString() : null
        };
      }

      EnvRequireRequest defaultRequest = new(ProjectRoot: null, ProjectMetadataFile: null);
      string arrangedProjectRoot = "/app/code";
      string arrangedProjectMetadata = "project-metadata.json";
      ProjectMetadata baseArrangedMetadata = new()
      {
        Name = Randomization.GuidString(),
        Title = Randomization.GuidString(),
        Version = $"{Randomization.Byte()}.{Randomization.Byte()}.{Randomization.Byte()}",
        CiEnvironment = new ProjectContinuousIntegrationEnvironmentDefinition
        {
          Variables = new[] {GenerateRandomEnvironmentVariable()}
        }
      };
      Func<string, string, string> combinePath = (path1, path2) => $"{path1}|{path2}";
      CommandDependencies dependencies = DependencyHelper.CreateMockDependencies() with
      {
        CombinePath = combinePath,
        DoesFileExist = path => path == combinePath(arrangedProjectRoot, arrangedProjectMetadata),
        EnsureDirectoryExists =
        path => path == arrangedProjectRoot
          ? new Result<string>(path)
          : new Result<string>(new Exception("Directory not found")),
        TryLoadFileString = path =>
          path == combinePath(arrangedProjectRoot, arrangedProjectMetadata)
            ? Json.TrySerialize(baseArrangedMetadata)
            : new Result<string>(new Exception("File not arranged"))
      };

      EnvRequireRequest happyPathProjectRoot = new(arrangedProjectRoot, ProjectMetadataFile: null);
      EnvRequireRequest happyPathFile =
        new(ProjectRoot: null, combinePath(arrangedProjectRoot, arrangedProjectMetadata));
      CommandDependencies happyPathDependencies = dependencies with
      {
        GetEnvironmentVariables = () => new Dictionary<string, string>(
          baseArrangedMetadata.CiEnvironment.Variables.Select(variable =>
            new KeyValuePair<string, string>(variable.Name, Randomization.GuidString()))
        )
      };
      Result<EnvRequireResult> happyPathResult =
        new(new EnvRequireResult(happyPathFile.ProjectMetadataFile!, baseArrangedMetadata));

      EnvRequireRequest sadPathEnvRequiredUnset = happyPathProjectRoot;
      ProjectMetadata sadPathEnvRequiredUnsetMetadata = baseArrangedMetadata with
      {
        CiEnvironment = baseArrangedMetadata.CiEnvironment with
        {
          Variables = baseArrangedMetadata.CiEnvironment.Variables
            .Select(variable => variable with {Required = true}).ToArray()
        }
      };
      CommandDependencies sadPathEnvRequiredUnsetDependencies = dependencies with
      {
        TryLoadFileString = path => path == combinePath(arrangedProjectRoot, arrangedProjectMetadata)
          ? Json.TrySerialize(sadPathEnvRequiredUnsetMetadata)
          : new Result<string>(new Exception("File not arranged"))
      };
      var sadPathEnvRequiredUnsetResult = new Result<EnvRequireResult>(
        new BadRequestException(
          $"Missing environment variables: {sadPathEnvRequiredUnsetMetadata.CiEnvironment.Variables.First().Name}"
        )
      );

      return new[]
      {
        TestCase(happyPathFile, happyPathResult, happyPathDependencies),
        TestCase(happyPathProjectRoot, happyPathResult, happyPathDependencies),
        TestCase(sadPathEnvRequiredUnset, sadPathEnvRequiredUnsetResult, sadPathEnvRequiredUnsetDependencies)
      };
    }

    [Theory]
    [MemberData(nameof(GenerateTestCases))]
    public async Task ReturnsExpectedResult(
      EnvRequireRequest request,
      Result<EnvRequireResult> expected,
      CommandDependencies dependencies
    )
    {
      var result = await EnvRequireHandling.TryHandleAsync(dependencies, request);

      Assertions.Results.Equal(expected, result);
    }
  }
}
