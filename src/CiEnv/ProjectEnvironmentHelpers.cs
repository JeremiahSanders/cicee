using System;
using System.Collections.Generic;
using System.Linq;
using Cicee.Commands;
using LanguageExt;
using LanguageExt.Common;

namespace Cicee.CiEnv;

public static class ProjectEnvironmentHelpers
{
  private const string secretString = "***redacted***";

  public static IReadOnlyDictionary<ProjectEnvironmentVariable, string> GetEnvironmentDisplay(
    Func<IReadOnlyDictionary<string, string>> getEnvironmentVariables,
    ProjectMetadata projectMetadata
  )
  {
    var knownEnvironment = getEnvironmentVariables();

    string GetVariableValue(ProjectEnvironmentVariable variable)
    {
      var possibleValue = knownEnvironment.FirstOrDefault(kvp =>
        kvp.Key.Equals(variable.Name, StringComparison.InvariantCultureIgnoreCase)
      );
      var hasValue = !default(KeyValuePair<string, string>).Equals(possibleValue);
      return hasValue
        ? variable.Secret ? secretString : possibleValue.Value
        : string.Empty;
    }

    return projectMetadata.CiEnvironment.Variables.ToDictionary(variable => variable, GetVariableValue);
  }

  public static Result<ProjectMetadata> ValidateEnvironment(
    Func<IReadOnlyDictionary<string, string>> getEnvironmentVariables,
    ProjectMetadata projectMetadata
  )
  {
    var knownEnvironment = getEnvironmentVariables();
    var knownVariables = knownEnvironment.Keys.ToArray();
    var missingVariables = projectMetadata.CiEnvironment.Variables
      .Where(envVariable => envVariable.Required && !knownVariables.Contains(envVariable.Name))
      .Select(envVariable => envVariable.Name)
      .OrderBy(Prelude.identity)
      .ToArray();

    return missingVariables.Any()
      ? new Result<ProjectMetadata>(
        new BadRequestException(
          $"Missing environment variables: {string.Join(", ", missingVariables)}"
        )
      )
      : new Result<ProjectMetadata>(projectMetadata);
  }

  public static void DisplayProjectEnvironmentValues(Action<string> standardOutWriteLine,
    Action<ConsoleColor?, string> standardOutWrite,
    IReadOnlyDictionary<ProjectEnvironmentVariable, string> environmentVariableDisplayValues)
  {
    void WriteEnvironmentVariables(IReadOnlyDictionary<ProjectEnvironmentVariable, string> environmentDisplay)
    {
      var width = environmentDisplay.Keys.Max(value => value.Name.Length) + 1;
      foreach (var (key, value) in environmentDisplay.OrderBy(kvp => kvp.Key.Name))
      {
        var isPopulated = value != string.Empty;
        ConsoleColor? nameColor = key.Required && !isPopulated ? ConsoleColor.Red : null;
        ConsoleColor? valueColor = key.Secret && isPopulated ? ConsoleColor.Yellow
          : isPopulated ? ConsoleColor.Blue
          : ConsoleColor.DarkGray;
        var valueOrDefault = isPopulated || string.IsNullOrWhiteSpace(key.DefaultValue)
          ? value // When the value is populated or no default exists, display value. 
          // If there is a default, but no value, and it's a secret, show the secret placeholder, else the default. 
          : $"<{(key.Secret ? secretString : key.DefaultValue)}>";
        
        standardOutWrite(nameColor, $"  {key.Name.PadRight(width, paddingChar: ' ')}");
        standardOutWrite(arg1: null, ": ");
        standardOutWrite(valueColor, valueOrDefault);
        standardOutWrite(arg1: null, Environment.NewLine);
      }
    }

    standardOutWriteLine("CI Environment:");
    if (environmentVariableDisplayValues.Any())
    {
      WriteEnvironmentVariables(environmentVariableDisplayValues);
    }
    else
    {
      standardOutWriteLine("  No CI environment variables defined.");
    }
  }
}
