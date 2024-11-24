using System.CommandLine;

using Cicee.Commands.Lib;

namespace Cicee.Commands;

public static class ShellOption
{
  private static readonly string[] Aliases =
  {
    "--shell",
    "-s"
  };

  public static Option<LibraryShellTemplate?> CreateOptional()
  {
    Option<LibraryShellTemplate?> option = new(Aliases, () => LibraryShellTemplate.Bash, description: "Shell template.")
    {
      IsRequired = false
    };
    return option;
  }

  public static Option<LibraryShellTemplate> CreateRequired()
  {
    Option<LibraryShellTemplate> option = new(Aliases, () => LibraryShellTemplate.Bash, description: "Shell template.")
    {
      IsRequired = true
    };
    return option;
  }
}
