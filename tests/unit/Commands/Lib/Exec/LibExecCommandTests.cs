using System;
using System.CommandLine.Binding;
using System.Linq;
using Cicee.Commands.Lib.Exec;
using LanguageExt.Common;
using Xunit;

namespace Cicee.Tests.Unit.Commands.Lib.Exec;

public class LibExecCommandTests
{
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
    var command = LibExecCommand.Create(dependencies);
    var expected = projectRoot;

    var actual = (command.Options.Single(option => option.Name == "project-root") as IValueDescriptor)
      .GetDefaultValue();

    Assert.Equal(expected, actual);
  }

  [Fact]
  public void InfersExpectedMetadataPath()
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
    var command = LibExecCommand.Create(dependencies);
    var expected = metadataFile;

    var actual = (command.Options.Single(option => option.Name == "metadata") as IValueDescriptor)
      .GetDefaultValue();

    Assert.Equal(expected, actual);
  }
}
