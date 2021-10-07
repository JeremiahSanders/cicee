using System.CommandLine;

namespace Cicee.Commands
{
  public static class ShellOption
  {
    public static Option Create()
    {
      var option = new Option<string>(new[] { "--shell", "-s" }, "Shell template.") { IsRequired = false };
      option.AddSuggestions("bash");
      return option;
    }
  }
}
