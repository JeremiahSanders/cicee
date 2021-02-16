using System.CommandLine;
using System.CommandLine.Invocation;

namespace Cicee.Commands.Exec
{
  public static class ExecCommand
  {
    private static Option CommandOption()
    {
      return new Option<string>(
        aliases: new[] {"--command", "-c"},
        description: "Execution command"
      ) {IsRequired = false};
    }

    private static Option EntrypointOption()
    {
      return new Option<string?>(
        aliases: new[] {"--entrypoint", "-e"},
        description: "Execution entrypoint"
      ) {IsRequired = false};
    }

    public static Command Create()
    {
      var command = new Command(name: "exec", description: "Execute")
      {
        ProjectRootOption.Create(), CommandOption(), EntrypointOption()
      };
      command.Handler = CommandHandler.Create<string, string?, string?>(ExecEntrypoint.HandleAsync);
      return command;
    }
  }
}
