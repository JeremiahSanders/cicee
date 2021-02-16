using System;
using System.Collections.Generic;
using System.Linq;
using Cicee.Commands;
using Cicee.Commands.Exec;
using LanguageExt;
using LanguageExt.Common;

namespace Cicee.CiEnv
{
  public static class ProjectEnvironmentHelpers
  {
    public static IReadOnlyDictionary<string, string> GetEnvironmentDisplay(
      Func<IReadOnlyDictionary<string, string>> getEnvironmentVariables,
      ExecRequestContext execRequestContext
    )
    {
      const string secretString = "********";
      var knownEnvironment = getEnvironmentVariables();
      var expectedEnvironment =
        execRequestContext.ProjectMetadata.CiEnvironment.Variables.Select(env => env.Name).ToArray();
      return new Dictionary<string, string>(
        collection: knownEnvironment
          .Where(keyValuePair => expectedEnvironment.Contains(keyValuePair.Key))
          .Select(keyValuePair =>
            execRequestContext.ProjectMetadata.CiEnvironment.Variables.First(envVar => envVar.Name == keyValuePair.Key)
              .Secret
              ? new KeyValuePair<string, string>(keyValuePair.Key, secretString)
              : keyValuePair)
      );
    }

    public static Result<ExecRequestContext> ValidateEnvironment(
      Func<IReadOnlyDictionary<string, string>> getEnvironmentVariables,
      ExecRequestContext execRequestContext
    )
    {
      var knownEnvironment = getEnvironmentVariables();
      string[] knownVariables = knownEnvironment.Keys.ToArray();
      string[] missingVariables = execRequestContext.ProjectMetadata.CiEnvironment.Variables
        .Where(envVariable => envVariable.Required && !knownVariables.Contains(envVariable.Name))
        .Select(envVariable => envVariable.Name)
        .OrderBy(Prelude.identity)
        .ToArray();

      return missingVariables.Any()
        ? new Result<ExecRequestContext>(
          e: new BadRequestException(
            message: $"Missing environment variables: {string.Join(separator: ", ", missingVariables)}"
          )
        )
        : new Result<ExecRequestContext>(execRequestContext);
    }
  }
}
