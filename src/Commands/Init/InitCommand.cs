using System.CommandLine;

namespace Cicee.Commands.Init;

public static class InitCommand
{
  private static Option<string?> ImageOption()
  {
    return new Option<string?>(
      new[] { "--image", "-i" },
      "Base CI image for $PROJECT_ROOT/ci/Dockerfile."
    ) { IsRequired = false };
  }

  public static Command Create()
  {
    var projectRoot = ProjectRootOption.Create();
    var image = ImageOption();
    var force = ForceOption.Create();
    var command =
      new Command("init", "Initialize project. Creates suggested cicee files.") { projectRoot, image, force };
    command.SetHandler<string, string?, bool>(InitEntrypoint.HandleAsync, projectRoot, image, force);
    return command;
  }
}
