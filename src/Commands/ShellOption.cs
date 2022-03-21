using System.Collections.Generic;
using System.CommandLine;

namespace Cicee.Commands;

public static class ShellOption
{
  private static readonly string[] SupportedShells = { "bash" };
  public static IReadOnlyList<string> Shells => SupportedShells;

  public static Option<string> Create()
  {
    var option = new Option<string>(
        new[] { "--shell", "-s" },
        () => string.Empty,
        "Shell template."
      ) { IsRequired = false }
      .FromAmong(SupportedShells);
    return option;
  }
}
