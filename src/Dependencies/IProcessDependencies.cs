using System;
using System.Threading.Tasks;

using LanguageExt.Common;

namespace Cicee.Dependencies;

/// <summary>
///   Dependencies which provide access to process execution.
/// </summary>
public interface IProcessDependencies
{
  /// <summary>Attempt to invoke a process and await its completion.</summary>
  Task<Result<ProcessExecResult>> ProcessExecutor(
    ProcessExecRequest processStartInfo,
    Action<string>? debugLogger = null);
}
