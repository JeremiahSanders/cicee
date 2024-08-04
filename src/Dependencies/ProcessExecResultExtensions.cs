using System;

namespace Cicee.Dependencies;

public static class ProcessExecResultExtensions
{
  public static ProcessExecResult RequireExitCodeZero(this ProcessExecResult processExecResult)
  {
    if (processExecResult.ExitCode != 0)
    {
      throw new InvalidOperationException($"Process returned non-zero exit code: {processExecResult.ExitCode}");
    }

    return processExecResult;
  }
}
