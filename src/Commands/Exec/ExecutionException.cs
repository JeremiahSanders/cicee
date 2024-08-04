using System;
using System.Diagnostics.CodeAnalysis;

namespace Cicee.Commands.Exec;

[ExcludeFromCodeCoverage]
public class ExecutionException : Exception
{
  public ExecutionException(string message, int exitCode, Exception? innerException = null)
    : base(message, innerException)
  {
    ExitCode = exitCode;
  }

  public int ExitCode { get; }
}
