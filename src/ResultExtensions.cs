using System;
using LanguageExt.Common;

namespace Cicee
{
  static class ResultExtensions
  {
    public static Result<NewSuccess> Bind<OldSuccess, NewSuccess>(this Result<OldSuccess> result, Func<OldSuccess, Result<NewSuccess>> binder)
    {
      return result.Match(binder, exception => new Result<NewSuccess>(exception));
    }
    public static Result<T> MapLeft<T>(this Result<T> result, Func<Exception, Exception> mapper)
    {
      return result.Match(value => result, exception => new Result<T>(mapper(exception)));
    }
  }
}
