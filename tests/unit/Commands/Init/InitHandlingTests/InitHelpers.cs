using Cicee.Commands.Init;
using LanguageExt.Common;

namespace Cicee.Tests.Unit.Commands.Init.InitHandlingTests
{
  public static class InitHelpers
  {
    public static InitDependencies CreateMockDependencies()
    {
      return new(
        (one, two) => $"{one}/{two}",
        (request, templateValues) => new Result<FileCopyRequest>(request),
        path => new Result<bool>(value: true),
        directoryPath => new Result<string>(directoryPath),
        () => "/temp/cicee/init/templates",
        message => { }
      );
    }
  }
}
