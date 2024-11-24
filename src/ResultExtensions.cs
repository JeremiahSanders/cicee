using Cicee.Commands.Exec;

using LanguageExt.Common;

namespace Cicee;

internal static class ResultExtensions
{
  public static int ToExitCode<T>(this Result<T> result, int failureCode = 1)
  {
    return result.Match(
      _ => 0,
      exception => exception switch
      {
        ExecutionException executionException => executionException.ExitCode,
        _ => failureCode
      }
    );
  }
}
