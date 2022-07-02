using System;
using System.Collections.Generic;
using Cicee.Commands;
using Cicee.Commands.Meta.Version.Bump;
using Jds.LanguageExt.Extras;
using LanguageExt.Common;
using Xunit;

namespace Cicee.Tests.Unit.Commands.Meta.Version.Bump.MetaVersionBumpHandlingTests;

public class TryBumpProjectVersionTests
{
  public const string PackageJsonStr = @"{
  ""name"": ""test-project"",
  ""version"": ""5.2.9"",
  ""description"": ""A Vue.js project"",
  ""main"": ""src/main.js"",
  ""private"": true,
  ""scripts"": {
    ""dev"": ""webpack-dev-server --inline --progress --config build/webpack.dev.conf.js"",
    ""start"": ""npm run dev"",
    ""unit"": ""jest --config test/unit/jest.conf.js --coverage"",
    ""test"": ""npm run unit"",
    ""lint"": ""eslint --ext .js,.vue src test/unit"",
    ""build"": ""node build/build.js""
  },
  ""dependencies"": {
    ""vue"": ""^2.5.2""
  },
  ""devDependencies"": {
    ""autoprefixer"": ""^7.1.2"",
    ""babel-core"": ""^6.22.1"",
    ""babel-eslint"": ""^8.2.1"",
    ""babel-helper-vue-jsx-merge-props"": ""^2.0.3"",
    ""babel-jest"": ""^21.0.2"",
    ""babel-loader"": ""^7.1.1"",
    ""babel-plugin-dynamic-import-node"": ""^1.2.0"",
    ""babel-plugin-syntax-jsx"": ""^6.18.0"",
    ""babel-plugin-transform-es2015-modules-commonjs"": ""^6.26.0"",
    ""babel-plugin-transform-runtime"": ""^6.22.0"",
    ""babel-plugin-transform-vue-jsx"": ""^3.5.0"",
    ""babel-preset-env"": ""^1.3.2"",
    ""babel-preset-stage-2"": ""^6.22.0"",
    ""chalk"": ""^2.0.1"",
    ""copy-webpack-plugin"": ""^4.0.1"",
    ""css-loader"": ""^0.28.0"",
    ""eslint"": ""^4.15.0"",
    ""eslint-config-airbnb-base"": ""^11.3.0"",
    ""eslint-friendly-formatter"": ""^3.0.0"",
    ""eslint-import-resolver-webpack"": ""^0.8.3"",
    ""eslint-loader"": ""^1.7.1"",
    ""eslint-plugin-import"": ""^2.7.0"",
    ""eslint-plugin-vue"": ""^4.0.0"",
    ""extract-text-webpack-plugin"": ""^3.0.0"",
    ""file-loader"": ""^1.1.4"",
    ""friendly-errors-webpack-plugin"": ""^1.6.1"",
    ""html-webpack-plugin"": ""^2.30.1"",
    ""jest"": ""^22.0.4"",
    ""jest-serializer-vue"": ""^0.3.0"",
    ""node-notifier"": ""^5.1.2"",
    ""optimize-css-assets-webpack-plugin"": ""^3.2.0"",
    ""ora"": ""^1.2.0"",
    ""portfinder"": ""^1.0.13"",
    ""postcss-import"": ""^11.0.0"",
    ""postcss-loader"": ""^2.0.8"",
    ""postcss-url"": ""^7.2.1"",
    ""rimraf"": ""^2.6.0"",
    ""semver"": ""^5.3.0"",
    ""shelljs"": ""^0.7.6"",
    ""uglifyjs-webpack-plugin"": ""^1.1.1"",
    ""url-loader"": ""^0.5.8"",
    ""vue-jest"": ""^1.0.2"",
    ""vue-loader"": ""^13.3.0"",
    ""vue-style-loader"": ""^3.0.1"",
    ""vue-template-compiler"": ""^2.5.2"",
    ""webpack"": ""^3.6.0"",
    ""webpack-bundle-analyzer"": ""^2.9.0"",
    ""webpack-dev-server"": ""^2.9.1"",
    ""webpack-merge"": ""^4.1.0""
  },
  ""engines"": {
    ""node"": "">= 6.0.0"",
    ""npm"": "">= 3.0.0""
  },
  ""browserslist"": [""> 1%"", ""last 2 versions"", ""not ie <= 8""]
}";

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
          ? new Result<string>(PackageJsonStr)
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