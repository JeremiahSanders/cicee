using System;
using System.Collections.Generic;
using System.Linq;

using Cicee.CiEnv;
using Cicee.Commands.Env.Display;
using Cicee.Dependencies;

using Jds.LanguageExt.Extras;

using LanguageExt;
using LanguageExt.Common;

using Xunit;

namespace Cicee.Tests.Unit.Commands.Env.Display.EnvDisplayHandlingTests;

public class TryHandleTests
{
  public static TryHandleTestArrangement CreateArrangement()
  {
    string arrangedMetadataPath = "/a/fake/path/package.json";
    string requiredVariable = "MOCK_REQUIRED";
    string optionalVariable = "MOCK_OPTIONAL";
    string missingVariable = "MOCK_MISSING";
    ProjectMetadata arrangedMetadata = new()
    {
      Name = "a-fake-project",
      Version = "2.1.4",
      CiEnvironment = new ProjectContinuousIntegrationEnvironmentDefinition
      {
        Variables = new[]
        {
          new ProjectEnvironmentVariable
          {
            Name = requiredVariable, Required = true
          },
          new ProjectEnvironmentVariable
          {
            Name = optionalVariable
          },
          new ProjectEnvironmentVariable
          {
            Name = missingVariable
          }
        }
      }
    };
    string packageJson = MockMetadata.GeneratePackageJson(arrangedMetadata);
    IReadOnlyDictionary<string, string> arrangedEnvironment = new Dictionary<string, string>
    {
      {
        requiredVariable, Guid.NewGuid().ToString()
      },
      {
        optionalVariable, Guid.NewGuid().ToString()
      }
    };
    CommandDependencies dependencies = DependencyHelper.CreateMockDependencies() with
    {
      TryLoadFileString = path => path == arrangedMetadataPath
        ? new Result<string>(packageJson)
        : new Result<string>(new Exception(message: "Path not arranged")),
      GetEnvironmentVariables = () => arrangedEnvironment
    };

    return new TryHandleTestArrangement(dependencies, arrangedMetadata, arrangedMetadataPath);
  }

  public static Result<EnvDisplayResponse> Act(TryHandleTestArrangement arrangement)
  {
    return EnvDisplayHandling.TryHandle(
      arrangement.Dependencies.EnsureFileExists,
      arrangement.Dependencies.TryLoadFileString,
      arrangement.Dependencies.GetEnvironmentVariables,
      arrangement.ArrangedMetadataPath
    );
  }

  [Fact]
  public void ReturnsExpectedEnvironmentValues()
  {
    TryHandleTestArrangement arrangement = CreateArrangement();
    IReadOnlyDictionary<string, string> variables = arrangement.Dependencies.GetEnvironmentVariables();
    Dictionary<ProjectEnvironmentVariable, string> expected =
      arrangement.ArrangedMetadata.CiEnvironment.Variables.ToDictionary(
        Prelude.identity,
        variable => variables.ContainsKey(variable.Name) ? variables[variable.Name] : string.Empty
      );

    Result<EnvDisplayResponse> result = Act(arrangement);
    IReadOnlyDictionary<ProjectEnvironmentVariable, string>? actual =
      result.Map(response => response.Environment).IfFailThrow();

    Assert.Equal(expected, actual);
  }

  public record TryHandleTestArrangement(
    CommandDependencies Dependencies,
    ProjectMetadata ArrangedMetadata,
    string ArrangedMetadataPath);
}
