using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cicee.CiEnv;
using Cicee.Commands.Exec;
using LanguageExt.Common;
using Xunit;

namespace Cicee.Tests.Unit.Commands.Exec.ExecHandlingTests
{
  public class CreateProcessStartInfo
  {
    private static ExecRequestContext CreateExecRequestContext()
    {
      return new(
        "/sample-project",
        new ProjectMetadata {CiEnvironment = new ProjectContinuousIntegrationEnvironmentDefinition()},
        "ls",
        "-al",
        "ci/ci.env",
        "ci/Dockerfile",
        Image: null
      );
    }

    public static IEnumerable<object[]> GenerateTestCases()
    {
      var baseDependencies = ExecHandlingTestHelpers.CreateDependencies();
      var baseRequest = CreateExecRequestContext();
      var baseExpectedResult = new ProcessStartInfoResult(
        "bash",
        "-c",
        baseDependencies.GetEnvironmentVariables()
      );

      var happyPathDependencies = baseDependencies;
      var happyPathRequest = baseRequest;
      var happyPathExpectedResult = baseExpectedResult with
      {
        Arguments =
        "-c \"" +
        $"CI_EXEC_CONTEXT=\\\"{Path.Combine(happyPathRequest.ProjectRoot, "ci")}\\\" " +
        $"PROJECT_ROOT=\\\"{happyPathRequest.ProjectRoot}\\\" " +
        $"LIB_ROOT=\\\"{happyPathDependencies.GetLibraryRootPath()}\\\" " +
        $"CI_COMMAND=\\\"{happyPathRequest.Command}\\\" " +
        $"CI_ENTRYPOINT=\\\"{happyPathRequest.Entrypoint}\\\" " +
        $"CI_ENV_INIT=\\\"{happyPathRequest.EnvironmentInitializationScriptPath}\\\" " +
        $"{Path.Combine(happyPathDependencies.GetLibraryRootPath(), "cicee-exec.sh")}\"",
        Environment = happyPathDependencies.GetEnvironmentVariables()
      };
      var happyPathExpected = new Result<ProcessStartInfoResult>(happyPathExpectedResult);

      IEnumerable<object[]> cases = new[] {new object[] {happyPathDependencies, happyPathRequest, happyPathExpected}};

      return cases;
    }

    [Theory]
    [MemberData(nameof(GenerateTestCases))]
    public void ReturnsExpectedProcessStartInfo(
      ExecDependencies dependencies,
      ExecRequestContext execRequestContext,
      Result<ProcessStartInfoResult> expectedResult
    )
    {
      var actualResult = ExecHandling.CreateProcessStartInfo(dependencies, execRequestContext)
        .Map(result => new ProcessStartInfoResult(result.FileName, result.Arguments,
          new Dictionary<string, string>(result.Environment)));

      expectedResult.IfSucc(expected =>
      {
        actualResult.IfSucc(actual =>
        {
          Assert.Equal(expected.FileName, actual.FileName);
          Assert.Equal(expected.Arguments, actual.Arguments);
          Assert.Equal(
            expected.Environment,
            // Selecting only those which are expected because actual keys will contain everything from the current test execution process (when ProcessStartInfo is created).
            actual.Environment.Where(kvp => expected.Environment.Keys.Contains(kvp.Key)),
            new KeyValuePairValueComparer<string, string>()
          );
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
          throw new Exception("Should have failed");
        });
        actualResult.IfFail(actual =>
        {
          Assert.Equal(exception.GetType(), actual.GetType());
          Assert.Equal(exception.Message, actual.Message);
        });
      });
    }

    public record ProcessStartInfoResult(string FileName, string Arguments,
      IReadOnlyDictionary<string, string> Environment);
  }
}
