using System;
using System.Collections.Generic;
using Cicee.Commands;
using Cicee.Commands.Exec;
using Cicee.Commands.Init;
using LanguageExt;
using LanguageExt.Common;

namespace Cicee.Tests.Unit.Commands
{
  public static class DependencyHelper
  {
    public static CommandDependencies CreateMockDependencies()
    {
      return CommandDependencies.Create()
        with
        {
          CombinePath = (path1, path2) => $"{path1}/{path2}",
          CopyTemplateToPath = (request, env) => new Result<FileCopyRequest>(request),
          DoesFileExist = path => new Result<bool>(value: true),
          EnsureDirectoryExists = path => new Result<string>(path),
          EnsureFileExists = path => new Result<string>(path),
          GetEnvironmentVariables = () => new Dictionary<string, string>(),
          GetInitTemplatesDirectoryPath = () => "/temp/cicee/init/templates",
          GetLibraryRootPath = () => "/cicee/lib",
          ProcessExecutor = info => new Result<ProcessExecResult>(new ProcessExecResult()).AsTask(),
          StandardErrorWriteLine = _ => { },
          StandardOutWriteLine = _ => { },
          TryLoadFileString = path => new Result<string>(new Exception($"Path {path} not arranged."))
        };
    }
  }
}
