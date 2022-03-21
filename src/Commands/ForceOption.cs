using System.CommandLine;

namespace Cicee.Commands
{
  public static class ForceOption
  {
    public static Option<bool> Create()
    {
      return new Option<bool>(
        new[] { "--force", "-f" },
        "Force writing files. Overwrites files which already exist."
      )
      { IsRequired = false };
    }
  }
}
