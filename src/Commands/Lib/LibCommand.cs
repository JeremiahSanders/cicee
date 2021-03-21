using System.CommandLine;
using System.CommandLine.Invocation;

namespace Cicee.Commands.Lib
{
  public static class LibCommand
  {
    private static Option CreateShellOption()
    {
      var option = new Option<string>(new[] {"--shell", "-s"}, "Shell template.") {IsRequired = false};
      option.AddSuggestions("bash");
      return option;
    }

    public static Command Create()
    {
      var command =
        new Command("lib",
          "Gets the path of the CICEE shell script library. Intended to be used as the target of 'source', i.e., 'source \"$(cicee lib --shell bash)\"'.")
        {
          CreateShellOption()
        };
      command.Handler = CommandHandler.Create<string?>(LibEntrypoint.HandleAsync);
      return command;
    }
  }
}
