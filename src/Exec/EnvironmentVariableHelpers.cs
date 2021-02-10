using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Cicee.Exec
{
  internal static class EnvironmentVariableHelpers
  {
    public static IDictionary<string, string> GetEnvironmentVariables()
    {
      return new Dictionary<string, string>(
        Environment.GetEnvironmentVariables()
          .Cast<DictionaryEntry>()
          .Select(de => new KeyValuePair<string, string>((string)de.Key, (string?)de.Value ?? string.Empty))
      );
    }
  }
}
