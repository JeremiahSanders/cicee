using System;

using LanguageExt.Common;

namespace Cicee;

public static class Require
{
  public static class AsResult
  {
    public static Result<string> NotNullOrWhitespace(string? possiblyNullString)
    {
      return string.IsNullOrWhiteSpace(possiblyNullString)
        ? new Result<string>(new ArgumentException(message: "Value is null or whitespace"))
        : new Result<string>(possiblyNullString!);
    }
  }
}
