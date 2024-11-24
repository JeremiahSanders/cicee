using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

using Cicee.CiEnv;
using Cicee.Commands.Meta.Version.Bump;
using Cicee.Dependencies;

using Jds.LanguageExt.Extras;

using LanguageExt.Common;

using Xunit;

namespace Cicee.Tests.Unit.Commands.Meta.Version.Bump.MetaVersionBumpHandlingTests;

public class TryBumpProjectVersionTests
{
  public static IEnumerable<object[]> CreateVersionTestCases()
  {
    const string arrangedMetadataPath = "/not-real/repo/package.json";
    System.Version currentVersion = new(major: 5, minor: 2, build: 9);
    System.Version expectedMajor = new(major: 6, minor: 0, build: 0);
    System.Version expectedMinor = new(major: 5, minor: 3, build: 0);
    System.Version expectedPatch = new(major: 5, minor: 2, build: 10);
    CommandDependencies dependencies = DependencyHelper.CreateMockDependencies() with
    {
      TryLoadFileString = path => path == arrangedMetadataPath
        ? new Result<string>(
          MockMetadata.GeneratePackageJson(
            new ProjectMetadata
            {
              Name = "fake-project", Version = currentVersion.ToString(fieldCount: 3)
            }
          )
        )
        : new Result<string>(new Exception(message: "Not found"))
    };

    yield return CreateTestCase(dependencies, arrangedMetadataPath, SemVerIncrement.Major, expectedMajor);
    yield return CreateTestCase(dependencies, arrangedMetadataPath, SemVerIncrement.Minor, expectedMinor);
    yield return CreateTestCase(dependencies, arrangedMetadataPath, SemVerIncrement.Patch, expectedPatch);
    yield break;

    object[] CreateTestCase(CommandDependencies commandDependencies, string projectMetadataPath,
      SemVerIncrement semVerIncrement, System.Version expectedVersion)
    {
      return new object[]
      {
        commandDependencies,
        projectMetadataPath,
        semVerIncrement,
        expectedVersion
      };
    }
  }

  [Theory]
  [MemberData(nameof(CreateVersionTestCases))]
  public void ReturnsExpectedVersion(CommandDependencies dependencies, string projectMetadataPath,
    SemVerIncrement semVerIncrement, System.Version expectedVersion)
  {
    (System.Version BumpedVersion, ProjectMetadata ProjectMetadata, JsonObject MetadataJson) actualTuple =
      MetaVersionBumpHandling.TryBumpProjectVersion(
        dependencies.TryLoadFileString,
        dependencies.EnsureFileExists,
        projectMetadataPath,
        semVerIncrement
      ).IfFailThrow();

    Assert.Equal(expectedVersion, actualTuple.BumpedVersion);
    Assert.Equal(expectedVersion.ToString(fieldCount: 3), actualTuple.ProjectMetadata.Version);
  }
}
