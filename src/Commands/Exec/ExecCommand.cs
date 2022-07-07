using System.CommandLine;
using Cicee.Dependencies;

namespace Cicee.Commands.Exec;

public static class ExecCommand
{
  private static Option<string> ServiceCommandOption()
  {
    return new Option<string>(
      new[] { "--command", "-c" },
      "Execution command"
    ) { IsRequired = false };
  }

  private static Option<string?> ServiceEntrypointOption()
  {
    return new Option<string?>(
      new[] { "--entrypoint", "-e" },
      "Execution entrypoint"
    ) { IsRequired = false };
  }

  private static Option<string?> ImageOption()
  {
    return new Option<string?>(
      new[] { "--image", "-i" },
      "Execution image. Overrides $PROJECT_ROOT/ci/Dockerfile."
    ) { IsRequired = false };
  }

  public static Command Create(CommandDependencies dependencies)
  {
    var projectRoot = ProjectRootOption.Create(dependencies);
    var serviceCommand = ServiceCommandOption();
    var serviceEntrypoint = ServiceEntrypointOption();
    var image = ImageOption();
    var command = new Command("exec", "Execute a command in a containerized execution environment.")
    {
      projectRoot, serviceCommand, serviceEntrypoint, image
    };
    command.SetHandler<string, string?, string?, string?>(
      ExecEntrypoint.HandleAsync,
      projectRoot,
      serviceCommand,
      serviceEntrypoint,
      image
    );
    return command;
  }
}
