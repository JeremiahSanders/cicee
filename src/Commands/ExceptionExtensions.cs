using System;

namespace Cicee.Commands;

internal static class ExceptionExtensions
{
  public static string ToExecutionFailureMessage(this Exception exception)
  {
    return $"Execution failed!\nReason: {exception.Message}";
  }
}
