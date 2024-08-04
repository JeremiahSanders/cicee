using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Linq;

using Cicee.Commands.Lib.Exec;
using Cicee.Dependencies;

using LanguageExt.Common;

using Xunit;

namespace Cicee.Tests.Unit.Commands.Lib.Exec;

public class LibExecCommandTests
{
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
    Command command = LibExecCommand.Create(dependencies);
    string expected = projectRoot;

    object? actual = (command.Options.Single(option => option.Name == "project-root") as IValueDescriptor)
      .GetDefaultValue();

    Assert.Equal(expected, actual);
  }

  [Fact]
  public void InfersExpectedMetadataPath()
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
    Command command = LibExecCommand.Create(dependencies);
    string expected = metadataFile;

    object? actual = (command.Options.Single(option => option.Name == "metadata") as IValueDescriptor)
      .GetDefaultValue();

    Assert.Equal(expected, actual);
  }
}
