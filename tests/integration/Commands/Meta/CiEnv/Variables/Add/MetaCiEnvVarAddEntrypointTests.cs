using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cicee.CiEnv;
using Cicee.Commands.Meta.CiEnv.Variables.Add;
using Cicee.Dependencies;
using Cicee.Tests.Unit;
using Cicee.Tests.Unit.Commands;
using Jds.LanguageExt.Extras;
using LanguageExt;
using LanguageExt.Common;
using Xunit;
using Xunit.Abstractions;

namespace Cicee.Tests.Integration.Commands.Meta.CiEnv.Variables.Add;

public static class MetaCiEnvVarAddEntrypointTests
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
    public async Task GivenNewVariable_AddsVariable()
    {
      List<(string FileName, string Content)> writeFileTargets = new();
      var knownMetadata = Guid.NewGuid().ToString("D");
      var projectMetadata = new ProjectMetadata
      {
        Name = Guid.NewGuid().ToString("N"),
        CiEnvironment = new ProjectContinuousIntegrationEnvironmentDefinition
        {
          Variables = new[]
          {
            new ProjectEnvironmentVariable
            {
              Name = Guid.NewGuid().ToString("D"), Required = true, Secret = false
            }
          }
        }
      };
      var dependencies = CreateDependencies(writeFileTargets, knownMetadata, projectMetadata);
      var newVariable = new ProjectEnvironmentVariable
      {
        DefaultValue = Guid.NewGuid().ToString("D"),
        Description = Guid.NewGuid().ToString("D"),
        Name = Guid.NewGuid().ToString("D"),
        Required = true,
        Secret = false
      };
      var expectedVariables = projectMetadata.CiEnvironment.Variables.Append(newVariable).ToArray();
      var expected = projectMetadata with
      {
        CiEnvironment = projectMetadata.CiEnvironment with { Variables = expectedVariables }
      };

      // Act
      var handler = MetaCiEnvVarAddEntrypoint.CreateHandler(dependencies);
      var exitCode = await handler(
        knownMetadata,
        newVariable.Name,
        newVariable.Description,
        newVariable.Required,
        newVariable.Secret,
        newVariable.DefaultValue
      );

      // Assert
      var (_, writtenContent) = writeFileTargets.Single(target => target.FileName == knownMetadata);
      var actual = Json.TryDeserialize<ProjectMetadata>(writtenContent).IfFailThrow();
      Assert.Equal(expected, actual);
      Assert.Equal(expected: 0, exitCode);
    }

    [Fact]
    public async Task GivenExistingVariable_Errors()
    {
      List<(string FileName, string Content)> writeFileTargets = new();
      var knownMetadata = Guid.NewGuid().ToString("D");
      var projectMetadata = new ProjectMetadata
      {
        Name = Guid.NewGuid().ToString("N"),
        CiEnvironment = new ProjectContinuousIntegrationEnvironmentDefinition
        {
          Variables = new[]
          {
            new ProjectEnvironmentVariable
            {
              Name = Guid.NewGuid().ToString("D"), Required = true, Secret = false
            }
          }
        }
      };
      var dependencies = CreateDependencies(writeFileTargets, knownMetadata, projectMetadata);
      var newVariable = new ProjectEnvironmentVariable
      {
        DefaultValue = Guid.NewGuid().ToString("D"),
        Description = Guid.NewGuid().ToString("D"),
        Name = projectMetadata.CiEnvironment.Variables[0].Name,
        Required = true,
        Secret = false
      };

      // Act
      var handler = MetaCiEnvVarAddEntrypoint.CreateHandler(dependencies);
      var exitCode = await handler(
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
