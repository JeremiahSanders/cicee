using System.CommandLine;

using Cicee.Commands.Lib;
using Cicee.Commands.Template.Lib;
using Cicee.Dependencies;

namespace Cicee.Commands.Init.Repository;

public static class RepositoryCommand
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

  private static Option<bool> InitCiLibOption()
  {
    return new Option<bool>(
      new[]
      {
        "--ci-lib",
        "-l"
      },
      () => false,
      TemplateLibCommand.Description
    )
    {
      IsRequired = false
    };
  }

  public static Command Create(CommandDependencies dependencies)
  {
    Option<string> projectRoot = ProjectRootOption.Create(dependencies);
    Option<string?> image = ImageOption();
    Option<bool> force = ForceOption.Create();
    Option<bool> initCiLib = InitCiLibOption();
    Option<LibraryShellTemplate?> shell = ShellOption.CreateOptional();
    Command command = new(
      name: "repository",
      description:
      "Initialize project repository. Creates suggested CICEE files and continuous integration scripts. Optionally includes CICEE CI library."
    )
    {
      projectRoot,
      image,
      force,
      initCiLib,
      shell
    };
    command.SetHandler(
      (root, imageName, forceValue, initValue, shellValue) => RepositoryEntrypoint.HandleAsync(
        dependencies,
        root,
        imageName,
        forceValue,
        initValue,
        shellValue
      ),
      projectRoot,
      image,
      force,
      initCiLib,
      shell
    );

    return command;
  }
}
