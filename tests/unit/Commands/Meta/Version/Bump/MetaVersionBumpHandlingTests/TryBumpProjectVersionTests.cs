using System;
using System.Collections.Generic;
using Cicee.CiEnv;
using Cicee.Commands;
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
    object[] CreateTestCase(
      CommandDependencies dependencies,
      string projectMetadataPath,
      SemVerIncrement semVerIncrement,
      System.Version expectedVersion
    )
    {
      return new object[] {dependencies, projectMetadataPath, semVerIncrement, expectedVersion};
    }


    var arrangedMetadataPath = "/not-real/repo/package.json";
    var currentVersion = new System.Version(major: 5, minor: 2, build: 9);
    var expectedMajor = new System.Version(major: 6, minor: 0, build: 0);
    var expectedMinor = new System.Version(major: 5, minor: 3, build: 0);
    var expectedPatch = new System.Version(major: 5, minor: 2, build: 10);
    var dependencies = DependencyHelper.CreateMockDependencies() with
    {
      TryLoadFileString = path =>
        path == arrangedMetadataPath
          ? new Result<string>(
            MockMetadata.GeneratePackageJson(new ProjectMetadata
            {
              Name = "fake-project", Version = currentVersion.ToString(fieldCount: 3)
            }))
          : new Result<string>(new Exception("Not found"))
    };

    yield return CreateTestCase(dependencies, arrangedMetadataPath, SemVerIncrement.Major, expectedMajor);
    yield return CreateTestCase(dependencies, arrangedMetadataPath, SemVerIncrement.Minor, expectedMinor);
    yield return CreateTestCase(dependencies, arrangedMetadataPath, SemVerIncrement.Patch, expectedPatch);
  }

  [Theory]
  [MemberData(nameof(CreateVersionTestCases))]
  public void ReturnsExpectedVersion(
    CommandDependencies dependencies,
    string projectMetadataPath,
    SemVerIncrement semVerIncrement,
    System.Version expectedVersion
  )
  {
    var actualTuple = MetaVersionBumpHandling.TryBumpProjectVersion(
        dependencies.TryLoadFileString,
        dependencies.EnsureFileExists,
        projectMetadataPath,
        semVerIncrement
      )
      .IfFailThrow();

    Assert.Equal(expectedVersion, actualTuple.BumpedVersion);
    Assert.Equal(expectedVersion.ToString(fieldCount: 3), actualTuple.ProjectMetadata.Version);
  }
}
