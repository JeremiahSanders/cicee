using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Cicee.Dependencies
{
  public static class EnvironmentVariableHelpers
  {
    public static IReadOnlyDictionary<string, string> GetEnvironmentVariables()
    {
      return new Dictionary<string, string>(
        collection: Environment.GetEnvironmentVariables()
          .Cast<DictionaryEntry>()
          .Select(de => new KeyValuePair<string, string>(key: (string)de.Key, value: (string?)de.Value ?? string.Empty))
      );
    }
  }
}
