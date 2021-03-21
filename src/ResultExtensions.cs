using System;
using System.Threading.Tasks;
using Cicee.Commands.Exec;
using LanguageExt;
using LanguageExt.Common;

namespace Cicee
{
  internal static class ResultExtensions
  {
    public static Result<NewSuccess> Bind<OldSuccess, NewSuccess>(
      this Result<OldSuccess> result,
      Func<OldSuccess, Result<NewSuccess>> binder
    )
    {
      return result.Match(binder, exception => new Result<NewSuccess>(exception));
    }

    public static Result<TSuccess> BindLeft<TSuccess>(
      this Result<TSuccess> result,
      Func<Exception, Result<TSuccess>> binder
    )
    {
      return result.Match(_ => result, binder);
    }

    public static Task<Result<NewSuccess>> BindAsync<OldSuccess, NewSuccess>(
      this Result<OldSuccess> result,
      Func<OldSuccess, Task<Result<NewSuccess>>> binder
    )
    {
      return result.Match(binder, exception => new Result<NewSuccess>(exception).AsTask());
    }

    /// <summary>
    ///   Executes <paramref name="action" /> within a try/catch, returning the input value.
    /// </summary>
    /// <param name="result"></param>
    /// <param name="action"></param>
    /// <typeparam name="TSuccess"></typeparam>
    /// <returns>Returns the value in <paramref name="result" /> upon success, or the <see cref="Exception" /> upon exception.</returns>
    public static Result<TSuccess> BindTryAction<TSuccess>(this Result<TSuccess> result, Action<TSuccess> action)
    {
      return result.Bind(value =>
        Prelude.Try(() =>
        {
          action(value);
          return value;
        }).Try()
      );
    }

    public static Result<NewSuccess> BindTry<OldSuccess, NewSuccess>(
      this Result<OldSuccess> result,
      Func<OldSuccess, NewSuccess> binder
    )
    {
      return result.Bind(value => Prelude.Try(() => binder(value)).Try());
    }

    public static Task<Result<NewSuccess>> BindTryAsync<OldSuccess, NewSuccess>(
      this Result<OldSuccess> result,
      Func<OldSuccess, Task<NewSuccess>> binder
    )
    {
      return result.BindAsync(value =>
        Prelude.TryAsync(() => binder(value)).Try()
      );
    }

    public static Result<T> MapLeft<T>(this Result<T> result, Func<Exception, Exception> mapper)
    {
      return result.Match(value => result, exception => new Result<T>(mapper(exception)));
    }

    public static Either<Exception, T> ToEither<T>(this Result<T> result)
    {
      return result.Match(Prelude.Right<Exception, T>, Prelude.Left<Exception, T>);
    }

    public static Result<T> Tap<T>(this Result<T> result, Action<T> action)
    {
      return result.Match(value =>
      {
        action(value);
        return result;
      }, _ => result);
    }

    public static Result<T> TapLeft<T>(this Result<T> result, Action<Exception> action)
    {
      return result.Match(_ => result,
        exception =>
        {
          action(exception);
          return result;
        });
    }

    public static int ToExitCode<T>(this Result<T> result, int failureCode = 1)
    {
      return result.Match(_ => 0, exception =>
        exception switch
        {
          ExecutionException executionException => executionException.ExitCode,
          _ => failureCode
        });
    }
  }
}
