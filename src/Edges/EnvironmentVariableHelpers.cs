using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Cicee.Edges;

public static class EnvironmentVariableHelpers
{
  /// <summary>
  ///   Retrieves all environment variables from the current process as a read-only dictionary.
  /// </summary>
  /// <remarks>Uses <see cref="System.Environment.GetEnvironmentVariables()" /></remarks>
  /// <returns>
  ///   A read-only dictionary containing all environment variables,
  ///   where the keys are variable names and the values are their corresponding values.
  /// </returns>
  /// <exception cref="System.Security.SecurityException">
  ///   The caller does not have the required permission to perform this operation.
  /// </exception>
  /// <exception cref="OutOfMemoryException">The buffer is out of memory.</exception>
  public static IReadOnlyDictionary<string, string> GetEnvironmentVariables()
  {
    return new Dictionary<string, string>(
      Environment
        .GetEnvironmentVariables()
        .Cast<DictionaryEntry>()
        .Select(de => new KeyValuePair<string, string>((string)de.Key, (string?)de.Value ?? string.Empty)
        )
    );
  }
}
