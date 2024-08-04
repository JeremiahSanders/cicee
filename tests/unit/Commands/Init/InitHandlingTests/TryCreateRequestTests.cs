using System.Collections.Generic;

using Cicee.Commands.Init;
using Cicee.Dependencies;

using LanguageExt.Common;

using Xunit;

namespace Cicee.Tests.Unit.Commands.Init.InitHandlingTests;

public class TryCreateRequestTests
{
  public static IEnumerable<object?[]> GenerateTestCases()
  {
    CommandDependencies happyPathDependencies = DependencyHelper.CreateMockDependencies();
    InitRequest happyPathRequest = new(ProjectRoot: "/media/cdrom/project", Image: null, OverwriteFiles: true);
    Result<InitRequest> happyPathExpected = new(happyPathRequest);

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

    object?[] CreateTestCase(CommandDependencies dependencies, string projectRoot, string? image, bool overwrite,
      Result<InitRequest> expected)
    {
      return new object?[]
      {
        dependencies,
        projectRoot,
        image,
        overwrite,
        expected
      };
    }
  }

  [Theory]
  [MemberData(nameof(GenerateTestCases))]
  public void ReturnsExpectedResult(CommandDependencies dependencies, string projectRoot, string? image, bool overwrite,
    Result<InitRequest> expected)
  {
    Result<InitRequest> actual = InitHandling.TryCreateRequest(dependencies, projectRoot, image, overwrite);

    Assertions.Results.Equal(expected, actual);
  }
}
