using System.Text.RegularExpressions;

namespace Cicee;

internal static class StringExtensions
{
  public static string ToKebabCase(this string value)
  {
    const string kebabSeparator = "-";
    Regex pattern = new(pattern: @"[A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+");
    return string.Join(kebabSeparator, pattern.Matches(value)).ToLower();
  }
}
