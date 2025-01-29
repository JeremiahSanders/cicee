using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Linq;

using Cicee.Commands.Exec;
using Cicee.Dependencies;

using LanguageExt.Common;

using Xunit;

namespace Cicee.Tests.Unit.Commands.Exec;

public static class ExecCommandTests
{
  public class Create
  {
    [Fact]
    public void ReturnsExpectedCommand()
    {
      CommandValues expected = new(
        Name: "exec",
        Description: "Execute a command in a containerized execution environment.",
        new[]
        {
          new OptionValues(
            Name: "project-root",
            Description: "Project repository root directory",
            new[]
            {
              "--project-root",
              "-p"
            },
            IsRequired: true
          ),
          new OptionValues(
            Name: "command",
            Description: "Execution command",
            new[]
            {
              "--command",
              "-c"
            },
            IsRequired: false
          ),
          new OptionValues(
            Name: "entrypoint",
            Description: "Execution entrypoint",
            new[]
            {
              "--entrypoint",
              "-e"
            },
            IsRequired: false
          ),
          new OptionValues(
            Name: "image",
            Description: "Execution image. Overrides $PROJECT_ROOT/ci/Dockerfile.",
            new[]
            {
              "--image",
              "-i"
            },
            IsRequired: false
          ),
          new OptionValues(
            Name: "harness",
            Description:
            "Invocation harness. Determines if CICEE directly invokes Docker commands or uses a shell script to invoke Docker commands.",
            new[]
            {
              "--harness",
              "-h"
            },
            IsRequired: false
          ),
          new OptionValues(
            Name: "verbosity",
            Description: "Execution progress verbosity. Only applicable when using 'Direct' harness.",
            new[]
            {
              "--verbosity",
              "-v"
            },
            IsRequired: false
          )
        }
      );

      CommandValues actual = CommandValues.FromCommand(ExecCommand.Create(DependencyHelper.CreateMockDependencies()));

      Assert.Equal(expected, actual);
    }

    [Fact]
    public void InfersExpectedProjectRoot()
    {
      string projectRoot = "/not/real/project";
      string currentDirectory = $"{projectRoot}/nested/folder";
      string metadataFile = $"{projectRoot}/package.json";
      CommandDependencies dependencies = DependencyHelper.CreateMockDependencies() with
      {
        TryGetCurrentDirectory = () => currentDirectory,
        TryLoadFileString = path => path == metadataFile
          ? new Result<string>(MockMetadata.GeneratePackageJson())
          : new Result<string>(new Exception(message: "path not arranged"))
      };
      Command command = ExecCommand.Create(dependencies);
      string expected = projectRoot;

      object? actual = (command.Options.Single(option => option.Name == "project-root") as IValueDescriptor)
        .GetDefaultValue();

      Assert.Equal(expected, actual);
    }
  }
}
