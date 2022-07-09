using System.CommandLine;
using Cicee.Commands.Lib;

namespace Cicee.Commands;

public static class ShellOption
{
  private static readonly string[] Aliases = {"--shell", "-s"};

  public static Option<LibraryShellTemplate?> CreateOptional()
  {
    var option = new Option<LibraryShellTemplate?>(
      Aliases,
      () => LibraryShellTemplate.Bash,
      "Shell template."
    ) {IsRequired = false};
    return option;
  }

  public static Option<LibraryShellTemplate> CreateRequired()
  {
    var option = new Option<LibraryShellTemplate>(
      Aliases,
      () => LibraryShellTemplate.Bash,
      "Shell template."
    ) {IsRequired = true};
    return option;
  }
}
