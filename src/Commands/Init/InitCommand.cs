using System.CommandLine;
using Cicee.Commands.Init.Repository;
using Cicee.Dependencies;

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

  public static Command Create(CommandDependencies dependencies)
  {
    var projectRoot = ProjectRootOption.Create(dependencies);
    var image = ImageOption();
    var force = ForceOption.Create();
    var command =
      new Command("init", "Initialize project. Creates suggested CICEE files.") { projectRoot, image, force };
    command.SetHandler<string, string?, bool>(InitEntrypoint.HandleAsync, projectRoot, image, force);

    command.AddCommand(RepositoryCommand.Create(dependencies));

    return command;
  }
}
