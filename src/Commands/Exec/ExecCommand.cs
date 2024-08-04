using System.CommandLine;

using Cicee.Dependencies;

namespace Cicee.Commands.Exec;

public static class ExecCommand
{
  private static Option<string> ServiceCommandOption()
  {
    return new Option<string>(
      new[]
      {
        "--command",
        "-c"
      },
      description: "Execution command"
    )
    {
      IsRequired = false
    };
  }

  private static Option<string?> ServiceEntrypointOption()
  {
    return new Option<string?>(
      new[]
      {
        "--entrypoint",
        "-e"
      },
      description: "Execution entrypoint"
    )
    {
      IsRequired = false
    };
  }

  private static Option<string?> ImageOption()
  {
    return new Option<string?>(
      new[]
      {
        "--image",
        "-i"
      },
      description: "Execution image. Overrides $PROJECT_ROOT/ci/Dockerfile."
    )
    {
      IsRequired = false
    };
  }

  public static Command Create(CommandDependencies dependencies)
  {
    Option<string> projectRoot = ProjectRootOption.Create(dependencies);
    Option<string> serviceCommand = ServiceCommandOption();
    Option<string?> serviceEntrypoint = ServiceEntrypointOption();
    Option<string?> image = ImageOption();
    Command command = new(name: "exec", description: "Execute a command in a containerized execution environment.")
    {
      projectRoot, serviceCommand, serviceEntrypoint, image
    };
    command.SetHandler<string, string?, string?, string?>(
      (rootValue, commandValue, entrypointValue, imageValue) => ExecEntrypoint.HandleAsync(
        dependencies,
        rootValue,
        commandValue,
        entrypointValue,
        imageValue
      ),
      projectRoot,
      serviceCommand,
      serviceEntrypoint,
      image
    );
    return command;
  }
}
