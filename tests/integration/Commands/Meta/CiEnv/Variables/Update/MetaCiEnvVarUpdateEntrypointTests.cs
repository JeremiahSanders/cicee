using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Cicee.CiEnv;
using Cicee.Commands.Meta.CiEnv.Variables.Update;
using Cicee.Dependencies;
using Cicee.Tests.Unit;
using Cicee.Tests.Unit.Commands;

using Jds.LanguageExt.Extras;

using LanguageExt;
using LanguageExt.Common;

using Xunit;
using Xunit.Abstractions;

namespace Cicee.Tests.Integration.Commands.Meta.CiEnv.Variables.Update;

public static class MetaCiEnvVarUpdateEntrypointTests
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
    public async Task GivenExistingVariable_ModifiesVariable()
    {
      List<(string FileName, string Content)> writeFileTargets = new();
      string knownMetadata = Guid.NewGuid().ToString(format: "D");
      ProjectEnvironmentVariable toModify = new()
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
            toModify,
            toLeave
          }
        }
      };
      CommandDependencies dependencies = CreateDependencies(writeFileTargets, knownMetadata, projectMetadata);
      ProjectEnvironmentVariable updatedVariable = toModify with
      {
        DefaultValue = Guid.NewGuid().ToString(format: "D"),
        Description = Guid.NewGuid().ToString(format: "D"),
        Required = !toModify.Required,
        Secret = !toModify.Secret
      };
      ProjectMetadata expected = projectMetadata with
      {
        CiEnvironment = projectMetadata.CiEnvironment with
        {
          Variables = new[]
          {
            updatedVariable,
            toLeave
          }
        }
      };

      // Act
      Func<string, string, string?, bool?, bool?, string?, Task<int>> handler =
        MetaCiEnvVarUpdateEntrypoint.CreateHandler(dependencies);
      int exitCode = await handler(
        knownMetadata,
        updatedVariable.Name,
        updatedVariable.Description,
        updatedVariable.Required,
        updatedVariable.Secret,
        updatedVariable.DefaultValue
      );

      // Assert
      (_, string writtenContent) = writeFileTargets.Single(target => target.FileName == knownMetadata);
      ProjectMetadata actual = Json.TryDeserialize<ProjectMetadata>(writtenContent).IfFailThrow();
      Assert.Equal(expected, actual);
      Assert.Equal(expected: 0, exitCode);
    }

    [Fact]
    public async Task GivenUnknownVariable_Errors()
    {
      List<(string FileName, string Content)> writeFileTargets = new();
      string knownMetadata = Guid.NewGuid().ToString(format: "D");
      ProjectMetadata projectMetadata = new()
      {
        Name = Guid.NewGuid().ToString(format: "N"),
        CiEnvironment = new ProjectContinuousIntegrationEnvironmentDefinition
        {
          Variables = new[]
          {
            new ProjectEnvironmentVariable
            {
              Name = Guid.NewGuid().ToString(format: "D"), Required = true, Secret = false
            }
          }
        }
      };
      CommandDependencies dependencies = CreateDependencies(writeFileTargets, knownMetadata, projectMetadata);
      ProjectEnvironmentVariable newVariable = new()
      {
        DefaultValue = Guid.NewGuid().ToString(format: "D"),
        Description = Guid.NewGuid().ToString(format: "D"),
        Name = Guid.NewGuid().ToString(format: "D"),
        Required = true,
        Secret = false
      };

      // Act
      Func<string, string, string?, bool?, bool?, string?, Task<int>> handler =
        MetaCiEnvVarUpdateEntrypoint.CreateHandler(dependencies);
      int exitCode = await handler(
        knownMetadata,
        newVariable.Name,
        newVariable.Description,
        newVariable.Required,
        newVariable.Secret,
        newVariable.DefaultValue
      );

      // Assert
      Assert.Empty(writeFileTargets);
      Assert.Equal(expected: 1, exitCode);
    }
  }
}
