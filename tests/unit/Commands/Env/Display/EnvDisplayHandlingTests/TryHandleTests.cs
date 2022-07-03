using System;
using System.Collections.Generic;
using System.Linq;
using Cicee.CiEnv;
using Cicee.Commands;
using Cicee.Commands.Env.Display;
using Jds.LanguageExt.Extras;
using LanguageExt;
using LanguageExt.Common;
using Xunit;

namespace Cicee.Tests.Unit.Commands.Env.Display.EnvDisplayHandlingTests;

public class TryHandleTests
{
  public static TryHandleTestArrangement CreateArrangement()
  {
    var arrangedMetadataPath = "/a/fake/path/package.json";
    var requiredVariable = "MOCK_REQUIRED";
    var optionalVariable = "MOCK_OPTIONAL";
    var missingVariable = "MOCK_MISSING";
    var arrangedMetadata = new ProjectMetadata
    {
      Name = "a-fake-project",
      Version = "2.1.4",
      CiEnvironment = new ProjectContinuousIntegrationEnvironmentDefinition
      {
        Variables = new[]
        {
          new ProjectEnvironmentVariable {Name = requiredVariable, Required = true},
          new ProjectEnvironmentVariable {Name = optionalVariable},
          new ProjectEnvironmentVariable {Name = missingVariable}
        }
      }
    };
    var packageJson = MockMetadata.GeneratePackageJson(arrangedMetadata);
    IReadOnlyDictionary<string, string> arrangedEnvironment = new Dictionary<string, string>
    {
      {requiredVariable, Guid.NewGuid().ToString()}, {optionalVariable, Guid.NewGuid().ToString()}
    };
    var dependencies = DependencyHelper.CreateMockDependencies() with
    {
      TryLoadFileString = path =>
        path == arrangedMetadataPath
          ? new Result<string>(packageJson)
          : new Result<string>(new Exception("Path not arranged")),
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
    var arrangement = CreateArrangement();
    var variables = arrangement.Dependencies.GetEnvironmentVariables();
    var expected = arrangement.ArrangedMetadata.CiEnvironment.Variables
      .ToDictionary(Prelude.identity,
        variable => variables.ContainsKey(variable.Name) ? variables[variable.Name] : string.Empty);

    var result = Act(arrangement);
    var actual = result.Map(response => response.Environment).IfFailThrow();

    Assert.Equal(expected, actual);
  }

  public record TryHandleTestArrangement(
    CommandDependencies Dependencies,
    ProjectMetadata ArrangedMetadata,
    string ArrangedMetadataPath
  );
}
