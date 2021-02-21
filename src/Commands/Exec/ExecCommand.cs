using System.CommandLine;
using System.CommandLine.Invocation;

namespace Cicee.Commands.Exec
{
  public static class ExecCommand
  {
    private static Option CommandOption()
    {
      return new Option<string>(
        new[] {"--command", "-c"},
        "Execution command"
      ) {IsRequired = false};
    }

    private static Option EntrypointOption()
    {
      return new Option<string?>(
        new[] {"--entrypoint", "-e"},
        "Execution entrypoint"
      ) {IsRequired = false};
    }

    private static Option ImageOption()
    {
      return new Option<string?>(
        new[] {"--image", "-i"},
        "Execution image. Overrides $PROJECT_ROOT/ci/Dockerfile."
      ) {IsRequired = false};
    }

    public static Command Create()
    {
      var command = new Command("exec", "Execute a command in a containerized execution environment.")
      {
        ProjectRootOption.Create(), CommandOption(), EntrypointOption(), ImageOption()
      };
      command.Handler = CommandHandler.Create<string, string?, string?, string?>(ExecEntrypoint.HandleAsync);
      return command;
    }
  }
}
