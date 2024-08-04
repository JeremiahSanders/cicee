using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using Cicee.CiEnv;
using Cicee.Commands.Exec;
using Cicee.Dependencies;

using Jds.LanguageExt.Extras;

using LanguageExt.Common;

using Xunit;

namespace Cicee.Tests.Unit.Commands.Exec.ExecHandlingTests;

public class CreateProcessStartInfo
{
  private static ExecRequestContext CreateExecRequestContext()
  {
    return new ExecRequestContext(
      ProjectRoot: "/sample-project",
      new ProjectMetadata
      {
        CiEnvironment = new ProjectContinuousIntegrationEnvironmentDefinition(),
        Name = "sample-project",
        Title = "Sample Project",
        Version = "0.7.2"
      },
      Command: "ls",
      Entrypoint: "-al",
      Dockerfile: "ci/Dockerfile",
      Image: null
    );
  }

  public static IEnumerable<object[]> GenerateTestCases()
  {
    CommandDependencies baseDependencies = DependencyHelper.CreateMockDependencies();
    ExecRequestContext baseRequest = CreateExecRequestContext();
    bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    string expectedExecutable = isWindows ? @"C:\Program Files\Git\bin\bash.exe" : "bash";
    ProcessStartInfoResult baseExpectedResult = new(
      expectedExecutable,
      Arguments: "-c",
      baseDependencies.GetEnvironmentVariables()
    );

    CommandDependencies happyPathDependencies = baseDependencies;
    ExecRequestContext happyPathRequest = baseRequest;
    ProcessStartInfoResult happyPathExpectedResult = baseExpectedResult with
    {
      Arguments =
      "-c \"" +
      $"{happyPathDependencies.CombinePath(happyPathDependencies.GetLibraryRootPath(), arg2: "cicee-exec.sh")}\"",
      Environment = happyPathDependencies.GetEnvironmentVariables()
    };
    Result<ProcessStartInfoResult> happyPathExpected = new(happyPathExpectedResult);

    IEnumerable<object[]> cases = new[]
    {
      new object[]
      {
        happyPathDependencies,
        happyPathRequest,
        happyPathExpected
      }
    };

    return cases;
  }

  [Theory]
  [MemberData(nameof(GenerateTestCases))]
  public void ReturnsExpectedProcessStartInfo(CommandDependencies dependencies, ExecRequestContext execRequestContext,
    Result<ProcessStartInfoResult> expectedResult)
  {
    Result<ProcessStartInfoResult> actualResult = ExecHandling.CreateProcessStartInfo(dependencies, execRequestContext)
      .Map(
        result => new ProcessStartInfoResult(
          result.FileName,
          result.Arguments,
          new Dictionary<string, string>(
            result.Environment.Map(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value ?? string.Empty))
          )
        )
      );

    expectedResult.IfSucc(
      expected =>
      {
        actualResult.IfSucc(
          actual =>
          {
            Assert.Equal(expected.FileName, actual.FileName);
            Assert.Equal(expected.Arguments, actual.Arguments);
            Assert.Equal(
              expected.Environment,
              // Selecting only those which are expected because actual keys will contain everything from the current test execution process (when ProcessStartInfo is created).
              actual.Environment.Where(kvp => expected.Environment.Keys.Contains(kvp.Key)),
              new KeyValuePairValueComparer<string, string>()
            );
          }
        );
        actualResult.IfFailThrow();
      }
    );
    expectedResult.IfFail(
      exception =>
      {
        actualResult.IfSucc(actual => throw new Exception(message: "Should have failed"));
        actualResult.IfFail(
          actual =>
          {
            Assert.Equal(exception.GetType(), actual.GetType());
            Assert.Equal(exception.Message, actual.Message);
          }
        );
      }
    );
  }

  public record ProcessStartInfoResult(
    string FileName,
    string Arguments,
    IReadOnlyDictionary<string, string> Environment);
}
