using System.Collections.Generic;
using Cicee.Commands;
using Cicee.Commands.Init;
using LanguageExt.Common;
using Xunit;

namespace Cicee.Tests.Unit.Commands.Init.InitHandlingTests
{
  public class TryCreateRequestTests
  {
    public static IEnumerable<object?[]> GenerateTestCases()
    {
      var happyPathDependencies = DependencyHelper.CreateMockDependencies();
      InitRequest happyPathRequest = new("/media/cdrom/project", Image: null, OverwriteFiles: true);
      var happyPathExpected = new Result<InitRequest>(happyPathRequest);

      object?[] CreateTestCase(
        CommandDependencies dependencies,
        string projectRoot,
        string? image,
        bool overwrite,
        Result<InitRequest> expected
      )
      {
        return new object?[] {dependencies, projectRoot, image, overwrite, expected};
      }

      return new[]
      {
        CreateTestCase(
          happyPathDependencies,
          happyPathRequest.ProjectRoot,
          happyPathRequest.Image,
          happyPathRequest.OverwriteFiles,
          happyPathExpected
        )
      };
    }

    [Theory]
    [MemberData(nameof(GenerateTestCases))]
    public void ReturnsExpectedResult(
      CommandDependencies dependencies,
      string projectRoot,
      string? image,
      bool overwrite,
      Result<InitRequest> expected
    )
    {
      var actual = InitHandling.TryCreateRequest(dependencies, projectRoot, image, overwrite);

      Assertions.Results.Equal(expected, actual);
    }
  }
}
