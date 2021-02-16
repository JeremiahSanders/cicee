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
        ProjectRoot: "/sample-project",
        ProjectMetadata: new ProjectMetadata {CiEnvironment = new ProjectContinuousIntegrationEnvironmentDefinition()},
        Command: "ls",
        Entrypoint: "-al",
        EnvironmentInitializationScriptPath: "ci/ci.env"
      );
    }

    public static IEnumerable<object[]> GenerateTestCases()
    {
      var baseDependencies = ExecHandlingTestHelpers.CreateDependencies();
      var baseRequest = CreateExecRequestContext();
      var baseExpectedResult = new ProcessStartInfoResult(
        FileName: "bash",
        Arguments: "-c",
        Environment: baseDependencies.GetEnvironmentVariables()
      );

      var happyPathDependencies = baseDependencies;
      var happyPathRequest = baseRequest;
      var happyPathExpectedResult = baseExpectedResult with
      {
        Arguments =
        $"-c \"PROJECT_ROOT=\\\"{happyPathRequest.ProjectRoot}\\\" LIB_ROOT=\\\"{happyPathDependencies.GetLibraryRootPath()}\\\" CI_COMMAND=\\\"{happyPathRequest.Command}\\\" CI_ENTRYPOINT=\\\"{happyPathRequest.Entrypoint}\\\" CI_ENV_INIT=\\\"{happyPathRequest.EnvironmentInitializationScriptPath}\\\" {Path.Combine(path1: happyPathDependencies.GetLibraryRootPath(), path2: "cicee-exec.sh")}\"",
        Environment = happyPathDependencies.GetEnvironmentVariables()
      };
      var happyPathExpected = new Result<ProcessStartInfoResult>(happyPathExpectedResult);

      IEnumerable<object[]> cases = new[] {new object[] {happyPathDependencies, happyPathRequest, happyPathExpected}};

      return cases;
    }

    [Theory]
    [MemberData(memberName: nameof(GenerateTestCases))]
    public void ReturnsExpectedProcessStartInfo(
      ExecDependencies dependencies,
      ExecRequestContext execRequestContext,
      Result<ProcessStartInfoResult> expectedResult
    )
    {
      var actualResult = ExecHandling.CreateProcessStartInfo(dependencies, execRequestContext)
        .Map(result => new ProcessStartInfoResult(result.FileName, result.Arguments,
          Environment: new Dictionary<string, string>(result.Environment)));

      expectedResult.IfSucc(expected =>
      {
        actualResult.IfSucc(actual =>
        {
          Assert.Equal(expected.FileName, actual.FileName);
          Assert.Equal(expected.Arguments, actual.Arguments);
          Assert.Equal(
            expected.Environment,
            // Selecting only those which are expected because actual keys will contain everything from the current test execution process (when ProcessStartInfo is created).
            actual: actual.Environment.Where(kvp => expected.Environment.Keys.Contains(kvp.Key)),
            comparer: new KeyValuePairValueComparer<string, string>()
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
          throw new Exception(message: "Should have failed");
        });
        actualResult.IfFail(actual =>
        {
          Assert.Equal(expected: exception.GetType(), actual: actual.GetType());
          Assert.Equal(exception.Message, actual.Message);
        });
      });
    }

    public record ProcessStartInfoResult(string FileName, string Arguments,
      IReadOnlyDictionary<string, string> Environment);
  }
}
