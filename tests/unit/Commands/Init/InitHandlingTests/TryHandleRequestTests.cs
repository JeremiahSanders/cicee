using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cicee.Commands.Init;
using LanguageExt.Common;
using Xunit;

namespace Cicee.Tests.Unit.Commands.Init.InitHandlingTests
{
  public class TryHandleRequestTests
  {
    public static IEnumerable<object[]> GenerateTestCases()
    {
      var happyPathDependencies = InitHelpers.CreateMockDependencies();
      InitRequest happyPathRequest = new("/media/cdrom/project", Image: null, OverwriteFiles: true);
      var happyPathExpected = new Result<InitRequest>(happyPathRequest);

      var sadPathDependencies = happyPathDependencies with
      {
        EnsureDirectoryExists = _ => new Result<string>(new DirectoryNotFoundException("Directory not found"))
      };
      var sadPathRequest = happyPathRequest;
      var sadPathExpected = new Result<InitRequest>(new DirectoryNotFoundException("Directory not found"));

      return new[]
      {
        new object[] {happyPathDependencies, happyPathRequest, happyPathExpected},
        new object[] {sadPathDependencies, sadPathRequest, sadPathExpected}
      };
    }

    [Theory]
    [MemberData(nameof(GenerateTestCases))]
    public async Task ReturnsExpectedResult(
      InitDependencies dependencies,
      InitRequest request,
      Result<InitRequest> expected
    )
    {
      var result = await InitHandling.TryHandleRequest(dependencies, request);

      Assertions.Results.Equal(expected, result);
    }
  }
}
