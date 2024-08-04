using System;
using System.CommandLine;

namespace Cicee.Commands;

public static class DryRunOption
{
  public static Option<bool> Of(Func<bool> getDefaultValue)
  {
    return new Option<bool>(
      new[]
      {
        "--dry-run",
        "-d"
      },
      getDefaultValue,
      description: "Execute a 'dry run', i.e., skip writing files and similar destructive steps."
    );
  }

  public static Option<bool> Create(bool defaultValue = false)
  {
    return Of(() => defaultValue);
  }
}
