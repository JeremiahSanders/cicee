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
      Task<Result<(string FileName, string Content)>> TryWriteFileStringAsync((string FileName, string Content) arg)
      {
        writeFileTargets.Add(arg);
        return new Result<(string FileName, string Content)>(arg).AsTask();
      }

      var dependencies = DependencyHelper.CreateMockDependencies() with
      {
        TryWriteFileStringAsync = TryWriteFileStringAsync,
        TryLoadFileString = path =>
          path == knownMetadata
            ? new Result<string>(
              MockMetadata.GeneratePackageJson(projectMetadata))
            : new Result<string>(new Exception("Not found"))
      };

      return dependencies;
    }

    [Fact]
    public async Task GivenNewVariable_Errors()
    {
      List<(string FileName, string Content)> writeFileTargets = new();
      var knownMetadata = Guid.NewGuid().ToString("D");
      var existing1 = new ProjectEnvironmentVariable
      {
        Name = Guid.NewGuid().ToString("D"), Required = true, Secret = false
      };
      var existing2 = new ProjectEnvironmentVariable
      {
        Name = Guid.NewGuid().ToString("D"), Required = false, Secret = false
      };
      var projectMetadata = new ProjectMetadata
      {
        Name = Guid.NewGuid().ToString("N"),
        CiEnvironment = new ProjectContinuousIntegrationEnvironmentDefinition
        {
          Variables = new[] { existing1, existing2 }
        }
      };
      var dependencies = CreateDependencies(writeFileTargets, knownMetadata, projectMetadata);

      // Act
      var handler = MetaCiEnvVarRemoveEntrypoint.CreateHandler(dependencies);
      var exitCode = await handler(
        knownMetadata,
        Guid.NewGuid().ToString()
      );

      // Assert
      Assert.Empty(writeFileTargets);
      Assert.Equal(expected: 1, exitCode);
    }

    [Fact]
    public async Task GivenExistingVariable_RemovesVariable()
    {
      List<(string FileName, string Content)> writeFileTargets = new();
      var knownMetadata = Guid.NewGuid().ToString("D");
      var toRemove = new ProjectEnvironmentVariable
      {
        Name = Guid.NewGuid().ToString("D"), Required = true, Secret = false
      };
      var toLeave = new ProjectEnvironmentVariable
      {
        Name = Guid.NewGuid().ToString("D"), Required = false, Secret = false
      };
      var projectMetadata = new ProjectMetadata
      {
        Name = Guid.NewGuid().ToString("N"),
        CiEnvironment = new ProjectContinuousIntegrationEnvironmentDefinition
        {
          Variables = new[] { toRemove, toLeave }
        }
      };
      var dependencies = CreateDependencies(writeFileTargets, knownMetadata, projectMetadata);

      // Act
      var handler = MetaCiEnvVarRemoveEntrypoint.CreateHandler(dependencies);
      var exitCode = await handler(
        knownMetadata,
        toRemove.Name
      );

      // Assert
      var expected = projectMetadata with
      {
        CiEnvironment = projectMetadata.CiEnvironment with { Variables = new[] { toLeave } }
      };
      var (_, writtenContent) = writeFileTargets.Single(target => target.FileName == knownMetadata);
      var actual = Json.TryDeserialize<ProjectMetadata>(writtenContent).IfFailThrow();
      Assert.Equal(expected, actual);
      Assert.Equal(expected: 0, exitCode);
    }
  }
}
