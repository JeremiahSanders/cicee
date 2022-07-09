using System;
using System.CommandLine.Binding;
using System.Linq;
using Cicee.Commands.Exec;
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
      var expected = new CommandValues(
        "exec",
        "Execute a command in a containerized execution environment.",
        new[]
        {
          new OptionValues(
            "project-root",
            "Project repository root directory",
            new[] {"--project-root", "-p"},
            IsRequired: true
          ),
          new OptionValues(
            "command",
            "Execution command",
            new[] {"--command", "-c"},
            IsRequired: false
          ),
          new OptionValues(
            "entrypoint",
            "Execution entrypoint",
            new[] {"--entrypoint", "-e"},
            IsRequired: false
          ),
          new OptionValues(
            "image",
            "Execution image. Overrides $PROJECT_ROOT/ci/Dockerfile.",
            new[] {"--image", "-i"},
            IsRequired: false
          )
        }
      );

      var actual = CommandValues.FromCommand(ExecCommand.Create(DependencyHelper.CreateMockDependencies()));

      Assert.Equal(expected, actual);
    }

    [Fact]
    public void InfersExpectedProjectRoot()
    {
      var projectRoot = "/not/real/project";
      var currentDirectory = $"{projectRoot}/nested/folder";
      var metadataFile = $"{projectRoot}/package.json";
      var dependencies = DependencyHelper.CreateMockDependencies() with
      {
        TryGetCurrentDirectory = () => currentDirectory,
        TryLoadFileString = path =>
          path == metadataFile
            ? new Result<string>(MockMetadata.GeneratePackageJson())
            : new Result<string>(new Exception("path not arranged"))
      };
      var command = ExecCommand.Create(dependencies);
      var expected = projectRoot;

      var actual = (command.Options.Single(option => option.Name == "project-root") as IValueDescriptor)
        .GetDefaultValue();

      Assert.Equal(expected, actual);
    }
  }
}
