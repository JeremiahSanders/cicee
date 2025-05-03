using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Cicee.Dependencies;
using Cicee.Edges.Filesystem;
using Cicee.Edges.Processes;

using LanguageExt.Common;

namespace Cicee.Commands.Exec.Handling;

public static class ScriptHarness
{
  public static Result<ProcessExecRequest> CreateProcessStartInfo(
    ICommandDependencies dependencies,
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
          ? BadRequestException.FromMessage($"Failed to find library file: {ciceeExecPath}")
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
