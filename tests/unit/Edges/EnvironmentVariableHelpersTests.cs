using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Cicee.Edges;

using Xunit;

namespace Cicee.Tests.Unit.Edges;

public static class EnvironmentVariableHelpersTests
{
  public class GetEnvironmentVariables
  {
    [Fact]
    public void ReturnsExecutionEnvironmentVariables()
    {
      IEnumerable<KeyValuePair<string, string>> expected = Environment
        .GetEnvironmentVariables()
        .Cast<DictionaryEntry>()
        .Select(de => new KeyValuePair<string, string>((string)de.Key, (string?)de.Value ?? string.Empty));

      IReadOnlyDictionary<string, string> actual = EnvironmentVariableHelpers.GetEnvironmentVariables();

      Assert.Equal<IEnumerable<KeyValuePair<string, string>>>(expected, actual);
    }
  }
}
