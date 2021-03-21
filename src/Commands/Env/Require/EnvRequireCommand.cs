using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace Cicee.Commands.Env.Require
{
  public static class EnvRequireCommand
  {
    private static Option CommandOption()
    {
      return new Option<string>(
        new[] {"--file", "-f"},
        "Project metadata file."
      ) {IsRequired = false};
    }

    private static Option OptionalProjectRootOption()
    {
      var option = ProjectRootOption.Create();
      option.IsRequired = false;
      return option;
    }

    public static Command Create()
    {
      var command =
        new Command("require", "Require that the environment contains all required variables.")
        {
          OptionalProjectRootOption(), CommandOption()
        };
      command.Handler = CommandHandler.Create<string?, string?>(EnvRequireEntrypoint.HandleAsync);
      return command;
    }
  }
}
