using System.CommandLine;

using Cicee.Commands.Init.Repository;
using Cicee.Dependencies;

namespace Cicee.Commands.Init;

public static class InitCommand
{
  private static Option<string?> ImageOption()
  {
    return new Option<string?>(
      new[]
      {
        "--image",
        "-i"
      },
      description: "Base CI image for $PROJECT_ROOT/ci/Dockerfile."
    )
    {
      IsRequired = false
    };
  }

  public static Command Create(ICommandDependencies dependencies)
  {
    Option<string> projectRoot = ProjectRootOption.Create(dependencies);
    Option<string?> image = ImageOption();
    Option<bool> force = ForceOption.Create();
    Command command = new(name: "init", description: "Initialize project. Creates suggested CICEE files.")
    {
      projectRoot, image, force
    };
    command.SetHandler(
      (root, imageName, forceValue) => InitEntrypoint.HandleAsync(dependencies, root, imageName, forceValue),
      projectRoot,
      image,
      force
    );

    command.AddCommand(RepositoryCommand.Create(dependencies));

    return command;
  }
}
