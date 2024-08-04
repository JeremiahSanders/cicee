using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Cicee.Commands.Init;
using Cicee.Dependencies;

using LanguageExt.Common;

using Xunit;

namespace Cicee.Tests.Unit.Commands.Init.InitHandlingTests;

public class TryHandleRequestTests
{
  public static IEnumerable<object[]> GenerateTestCases()
  {
    CommandDependencies happyPathDependencies = DependencyHelper.CreateMockDependencies();
    InitRequest happyPathRequest = new(ProjectRoot: "/media/cdrom/project", Image: null, OverwriteFiles: true);
    Result<InitRequest> happyPathExpected = new(happyPathRequest);

    CommandDependencies sadPathDependencies = happyPathDependencies with
    {
      EnsureDirectoryExists = _ => new Result<string>(new DirectoryNotFoundException(message: "Directory not found"))
    };
    InitRequest sadPathRequest = happyPathRequest;
    Result<InitRequest> sadPathExpected = new(new DirectoryNotFoundException(message: "Directory not found"));

    return new[]
    {
      new object[]
      {
        happyPathDependencies,
        happyPathRequest,
        happyPathExpected
      },
      new object[]
      {
        sadPathDependencies,
        sadPathRequest,
        sadPathExpected
      }
    };
  }

  [Theory]
  [MemberData(nameof(GenerateTestCases))]
  public async Task ReturnsExpectedResult(CommandDependencies dependencies, InitRequest request,
    Result<InitRequest> expected)
  {
    Result<InitRequest> result = await InitHandling.TryHandleRequest(dependencies, request);

    Assertions.Results.Equal(expected, result);
  }
}
