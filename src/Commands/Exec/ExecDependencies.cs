using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LanguageExt.Common;

namespace Cicee.Commands.Exec
{
  [ExcludeFromCodeCoverage]
  public record ExecDependencies(
    Func<string, Result<string>> EnsureDirectoryExists,
    Func<string, Result<string>> EnsureFileExists,
    Func<IReadOnlyDictionary<string, string>> GetEnvironmentVariables,
    Func<string> GetLibraryRootPath,
    Func<ProcessStartInfo, Task<Result<ProcessExecResult>>> ProcessExecutor,
    Action<int> SetExitCode,
    Action<string> StandardOutWriteLine,
    Func<string, Result<string>> TryLoadFileString
  );
}
