using System.CommandLine;
using System.CommandLine.Invocation;
using Cicee.Commands.Init.Template;

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

    public static Command Create()
    {
      var command =
        new Command("init", "Initialize project. Creates suggested cicee files.")
        {
          ProjectRootOption.Create(), ImageOption(), ForceOption.Create()
        };
      command.AddCommand(TemplateCommand.Create());
      command.Handler = CommandHandler.Create<string, string?, bool>(InitEntrypoint.HandleAsync);
      return command;
    }
  }
}
