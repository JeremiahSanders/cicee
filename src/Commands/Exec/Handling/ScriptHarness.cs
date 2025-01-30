using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Cicee.Dependencies;

using LanguageExt.Common;

namespace Cicee.Commands.Exec.Handling;

public static class ScriptHarness
{
  public static Result<ProcessStartInfo> CreateProcessStartInfo(
    CommandDependencies dependencies,
    ExecRequestContext execRequestContext
  )
  {
    string ciceeExecPath = dependencies.CombinePath(
      dependencies.GetLibraryRootPath(),
      HandlingConstants.CiceeExecScriptName
    );

    return dependencies
      .EnsureFileExists(ciceeExecPath)
      .MapFailure(
        exception => exception is FileNotFoundException
          ? new BadRequestException($"Failed to find library file: {ciceeExecPath}")
          : exception
      )
      .Bind(
        validatedCiceeExecPath =>
        {
          string ciceeExecLinuxPath = Io.NormalizeToLinuxPath(validatedCiceeExecPath);

          return ProcessHelpers.TryCreateBashProcessStartInfo(
            IoEnvironment.GetExecEnvironment(dependencies, execRequestContext, true),
            new Dictionary<string, string>(),
            ciceeExecLinuxPath
          );
        }
      );
  }
}
