using System;

using Cicee.CiEnv;
using Cicee.Dependencies;

using Jds.LanguageExt.Extras;

namespace Cicee.Tests.Unit;

public static class MockMetadata
{
  public const string DefaultName = "fake-project";
  public const string DefaultVersion = "5.1.3";
  public const string DefaultDescription = "A fake project to show how a CI project environment may be defined.";

  public static string GeneratePackageJson(ProjectMetadata? sourceMetadata = null)
  {
    ProjectContinuousIntegrationEnvironmentDefinition? possibleCiEnvironment = sourceMetadata?.CiEnvironment;
    string ciEnvironmentJson = possibleCiEnvironment == null
      ? string.Empty
      : $",{Environment.NewLine}  \"ciEnvironment\": {Json.TrySerialize(possibleCiEnvironment).IfFailThrow()}";
    return $@"{{
  ""name"": ""{sourceMetadata?.Name ?? DefaultName}"",
  ""version"": ""{sourceMetadata?.Version ?? DefaultVersion}"",
  ""description"": ""{sourceMetadata?.Description ?? DefaultDescription}"",
  ""main"": ""package-entrypoint-script.js"",
  ""scripts"": {{
    ""test"": ""npx run-test-script""
  }},
  ""repository"": {{
    ""type"": ""git"",
    ""url"": ""https://not-real/gitrepo.git""
  }},
  ""keywords"": [
    ""mock"",
    ""packagejson""
  ],
  ""author"": ""cicee"",
  ""license"": ""MIT""{ciEnvironmentJson}
}}";
  }
}
