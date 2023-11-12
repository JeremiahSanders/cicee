using System;
using System.Collections.Generic;
using Cicee.CiEnv;
using Cicee.Commands.Meta.Version.Set;
using Cicee.Dependencies;
using Jds.LanguageExt.Extras;
using LanguageExt.Common;
using Xunit;

namespace Cicee.Tests.Unit.Commands.Meta.Version.Set.MetaVersionSetHandlingTests;

public class TrySetProjectVersionTests
{
  public static IEnumerable<object[]> CreateVersionTestCases()
  {
    object[] CreateTestCase(
      CommandDependencies dependencies,
      string projectMetadataPath,
      System.Version version,
      System.Version expectedVersion
    )
    {
      return new object[] {dependencies, projectMetadataPath, version, expectedVersion};
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

    yield return CreateTestCase(dependencies, arrangedMetadataPath, expectedMajor, expectedMajor);
    yield return CreateTestCase(dependencies, arrangedMetadataPath, expectedMinor, expectedMinor);
    yield return CreateTestCase(dependencies, arrangedMetadataPath, expectedPatch, expectedPatch);
  }

  [Theory]
  [MemberData(nameof(CreateVersionTestCases))]
  public void ReturnsExpectedVersion(
    CommandDependencies dependencies,
    string projectMetadataPath,
    System.Version version,
    System.Version expectedVersion
  )
  {
    var actualTuple = MetaVersionSetHandling.TrySetProjectVersion(
        dependencies.TryLoadFileString,
        dependencies.EnsureFileExists,
        projectMetadataPath,
        version
      )
      .IfFailThrow();

    Assert.Equal(expectedVersion, actualTuple.Version);
    Assert.Equal(expectedVersion.ToString(fieldCount: 3), actualTuple.ProjectMetadata.Version);
  }
}
