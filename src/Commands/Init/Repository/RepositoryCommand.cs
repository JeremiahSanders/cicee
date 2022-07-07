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
      new[] { "--image", "-i" },
      "Base CI image for $PROJECT_ROOT/ci/Dockerfile."
    ) { IsRequired = false };
  }

  private static Option<bool> InitCiLibOption()
  {
    return new Option<bool>(
      new[] { "--ci-lib", "-l" },
      () => false,
      TemplateLibCommand.Description
    ) { IsRequired = false };
  }

  public static Command Create(CommandDependencies dependencies)
  {
    var projectRoot = ProjectRootOption.Create(dependencies);
    var image = ImageOption();
    var force = ForceOption.Create();
    var initCiLib = InitCiLibOption();
    var shell = ShellOption.CreateOptional();
    var command =
      new Command("repository",
        "Initialize project repository. Creates suggested CICEE files and continuous integration scripts. Optionally includes CICEE CI library.")
      {
        projectRoot,
        image,
        force,
        initCiLib,
        shell
      };
    command.SetHandler<string, string?, bool, bool, LibraryShellTemplate?>(
      RepositoryEntrypoint.HandleAsync,
      projectRoot,
      image,
      force,
      initCiLib,
      shell
    );

    return command;
  }
}
