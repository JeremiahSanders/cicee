using System;
using System.Threading.Tasks;
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

    public static Task<Result<NewSuccess>> BindAsync<OldSuccess, NewSuccess>(
      this Result<OldSuccess> result,
      Func<OldSuccess, Task<Result<NewSuccess>>> binder
    )
    {
      return result.Match(binder, exception => new Result<NewSuccess>(exception).AsTask());
    }

    public static Result<T> MapLeft<T>(this Result<T> result, Func<Exception, Exception> mapper)
    {
      return result.Match(value => result, exception => new Result<T>(mapper(exception)));
    }

    public static Either<Exception, T> ToEither<T>(this Result<T> result)
    {
      return result.Match(Prelude.Right<Exception,T>,Prelude.Left<Exception,T>);
    }
  }
}
