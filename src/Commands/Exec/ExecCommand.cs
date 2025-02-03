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

  private static Option<ExecInvocationHarness> InvocationHarnessOption()
  {
    return new Option<ExecInvocationHarness>(
      new[]
      {
        "--harness",
        "-h"
      },
      () => ExecInvocationHarness.Script,
      description:
      "Invocation harness. Determines if CICEE directly invokes Docker commands or uses a shell script to invoke Docker commands."
    )
    {
      IsRequired = false
    };
  }

  private static Option<ExecVerbosity> VerbosityOption()
  {
    return new Option<ExecVerbosity>(
      new[]
      {
        "--verbosity",
        "-v"
      },
      () => ExecVerbosity.Normal,
      description:
      "Execution progress verbosity. Only applicable when using 'Direct' harness."
    )
    {
      IsRequired = false
    };
  }

  public static Command Create(ICommandDependencies dependencies)
  {
    Option<string> projectRoot = ProjectRootOption.Create(dependencies);
    Option<string> serviceCommand = ServiceCommandOption();
    Option<string?> serviceEntrypoint = ServiceEntrypointOption();
    Option<string?> image = ImageOption();
    Option<ExecInvocationHarness> harness = InvocationHarnessOption();
    Option<ExecVerbosity> verbosity = VerbosityOption();
    Command command = new(name: "exec", description: "Execute a command in a containerized execution environment.")
    {
      projectRoot,
      serviceCommand,
      serviceEntrypoint,
      image,
      harness,
      verbosity
    };
    command.SetHandler<string, string?, string?, string?, ExecInvocationHarness, ExecVerbosity>(
      (rootValue, commandValue, entrypointValue, imageValue, harnessValue, verbosityValue) => ExecEntrypoint.HandleAsync(
        dependencies,
        rootValue,
        commandValue,
        entrypointValue,
        imageValue,
        harnessValue,
        verbosityValue
      ),
      projectRoot,
      serviceCommand,
      serviceEntrypoint,
      image,
      harness,
      verbosity
    );
    return command;
  }
}
