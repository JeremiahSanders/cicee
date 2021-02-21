using System.CommandLine;
using System.CommandLine.Invocation;

namespace Cicee.Commands.Init
{
  public static class InitCommand
  {
    private static Option ImageOption()
    {
      return new Option<string?>(
        new[] {"--image", "-i"},
        "Base CI image for $PROJECT_ROOT/ci/Dockerfile."
      ) {IsRequired = false};
    }

    private static Option ForceOption()
    {
      return new Option<bool>(
        new[] {"--force", "-f"},
        "Force writing files. Overwrites files which already exist."
      ) {IsRequired = false};
    }

    public static Command Create()
    {
      var command =
        new Command("init", "Initialize project. Creates suggested cicee files.")
        {
          ProjectRootOption.Create(), ImageOption(), ForceOption()
        };
      command.Handler = CommandHandler.Create<string, string?, bool>(InitEntrypoint.HandleAsync);
      return command;
    }
  }
}
