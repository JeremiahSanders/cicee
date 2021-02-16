using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cicee.Commands.Exec;
using Xunit;

namespace Cicee.Tests.Unit.Commands.Exec
{
  public static class EnvironmentVariableHelpersTests
  {
    public class GetEnvironmentVariables
    {
      [Fact]
      public void ReturnsExecutionEnvironmentVariables()
      {
        var expected = Environment.GetEnvironmentVariables()
          .Cast<DictionaryEntry>()
          .Select(de =>
            new KeyValuePair<string, string>(key: (string)de.Key, value: (string?)de.Value ?? string.Empty)
          );

        var actual = EnvironmentVariableHelpers.GetEnvironmentVariables();

        Assert.Equal<IEnumerable<KeyValuePair<string, string>>>(expected, actual);
      }
    }
  }
}
