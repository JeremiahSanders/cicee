using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Cicee.CiEnv;
using Cicee.Commands.Meta.CiEnv.Variables.Remove;
using Cicee.Dependencies;
using Cicee.Tests.Unit;
using Cicee.Tests.Unit.Commands;

using Jds.LanguageExt.Extras;

using LanguageExt;
using LanguageExt.Common;

using Xunit;
using Xunit.Abstractions;

namespace Cicee.Tests.Integration.Commands.Meta.CiEnv.Variables.Remove;

public static class MetaCiEnvVarRemoveEntrypointTests
{
  public class CreateHandlerTests
  {
    private readonly ITestOutputHelper _testOutputHelper;

    public CreateHandlerTests(ITestOutputHelper testOutputHelper)
    {
      _testOutputHelper = testOutputHelper;
    }

    private CommandDependencies CreateDependencies(List<(string FileName, string Content)> writeFileTargets,
      string knownMetadata, ProjectMetadata? projectMetadata)
    {
      CommandDependencies dependencies = DependencyHelper.CreateMockDependencies() with
      {
        TryWriteFileStringAsync = TryWriteFileStringAsync,
        TryLoadFileString = path => path == knownMetadata
          ? new Result<string>(MockMetadata.GeneratePackageJson(projectMetadata))
          : new Result<string>(new Exception(message: "Not found"))
      };

      return dependencies;

      Task<Result<(string FileName, string Content)>> TryWriteFileStringAsync((string FileName, string Content) arg)
      {
        writeFileTargets.Add(arg);
        return new Result<(string FileName, string Content)>(arg).AsTask();
      }
    }

    [Fact]
    public async Task GivenNewVariable_Errors()
    {
      List<(string FileName, string Content)> writeFileTargets = new();
      string knownMetadata = Guid.NewGuid().ToString(format: "D");
      ProjectEnvironmentVariable existing1 = new()
      {
        Name = Guid.NewGuid().ToString(format: "D"), Required = true, Secret = false
      };
      ProjectEnvironmentVariable existing2 = new()
      {
        Name = Guid.NewGuid().ToString(format: "D"), Required = false, Secret = false
      };
      ProjectMetadata projectMetadata = new()
      {
        Name = Guid.NewGuid().ToString(format: "N"),
        CiEnvironment = new ProjectContinuousIntegrationEnvironmentDefinition
        {
          Variables = new[]
          {
            existing1,
            existing2
          }
        }
      };
      CommandDependencies dependencies = CreateDependencies(writeFileTargets, knownMetadata, projectMetadata);

      // Act
      Func<string, string, Task<int>> handler = MetaCiEnvVarRemoveEntrypoint.CreateHandler(dependencies);
      int exitCode = await handler(knownMetadata, Guid.NewGuid().ToString());

      // Assert
      Assert.Empty(writeFileTargets);
      Assert.Equal(expected: 1, exitCode);
    }

    [Fact]
    public async Task GivenExistingVariable_RemovesVariable()
    {
      List<(string FileName, string Content)> writeFileTargets = new();
      string knownMetadata = Guid.NewGuid().ToString(format: "D");
      ProjectEnvironmentVariable toRemove = new()
      {
        Name = Guid.NewGuid().ToString(format: "D"), Required = true, Secret = false
      };
      ProjectEnvironmentVariable toLeave = new()
      {
        Name = Guid.NewGuid().ToString(format: "D"), Required = false, Secret = false
      };
      ProjectMetadata projectMetadata = new()
      {
        Name = Guid.NewGuid().ToString(format: "N"),
        CiEnvironment = new ProjectContinuousIntegrationEnvironmentDefinition
        {
          Variables = new[]
          {
            toRemove,
            toLeave
          }
        }
      };
      CommandDependencies dependencies = CreateDependencies(writeFileTargets, knownMetadata, projectMetadata);

      // Act
      Func<string, string, Task<int>> handler = MetaCiEnvVarRemoveEntrypoint.CreateHandler(dependencies);
      int exitCode = await handler(knownMetadata, toRemove.Name);

      // Assert
      ProjectMetadata expected = projectMetadata with
      {
        CiEnvironment = projectMetadata.CiEnvironment with
        {
          Variables = new[]
          {
            toLeave
          }
        }
      };
      (_, string writtenContent) = writeFileTargets.Single(target => target.FileName == knownMetadata);
      ProjectMetadata actual = Json.TryDeserialize<ProjectMetadata>(writtenContent).IfFailThrow();
      Assert.Equal(expected, actual);
      Assert.Equal(expected: 0, exitCode);
    }
  }
}
