using System;
using System.Collections.Generic;
using System.Linq;

using Cicee.Commands;

using LanguageExt;
using LanguageExt.Common;

namespace Cicee.CiEnv;

public static class ProjectEnvironmentHelpers
{
  internal const string SecretString = "***redacted***";

  public static IReadOnlyDictionary<ProjectEnvironmentVariable, string> GetEnvironmentDisplay(
    Func<IReadOnlyDictionary<string, string>> getEnvironmentVariables, ProjectMetadata projectMetadata)
  {
    IReadOnlyDictionary<string, string> knownEnvironment = getEnvironmentVariables();

    return projectMetadata.CiEnvironment.Variables.ToDictionary(variable => variable, GetVariableValue);

    string GetVariableValue(ProjectEnvironmentVariable variable)
    {
      KeyValuePair<string, string> possibleValue = knownEnvironment.FirstOrDefault(
        kvp => kvp.Key.Equals(variable.Name, StringComparison.InvariantCultureIgnoreCase)
      );
      bool hasValue = !default(KeyValuePair<string, string>).Equals(possibleValue);
      return hasValue ? variable.Secret ? SecretString : possibleValue.Value : string.Empty;
    }
  }

  public static Result<ProjectMetadata> ValidateEnvironment(
    Func<IReadOnlyDictionary<string, string>> getEnvironmentVariables, ProjectMetadata projectMetadata)
  {
    IReadOnlyDictionary<string, string> knownEnvironment = getEnvironmentVariables();
    string[] knownVariables = knownEnvironment.Keys.ToArray();
    string[] missingVariables = projectMetadata.CiEnvironment.Variables
      .Where(envVariable => envVariable.Required && !knownVariables.Contains(envVariable.Name))
      .Select(envVariable => envVariable.Name).OrderBy(Prelude.identity).ToArray();

    return missingVariables.Any()
      ? new Result<ProjectMetadata>(
        new BadRequestException($"Missing environment variables: {string.Join(separator: ", ", missingVariables)}")
      )
      : new Result<ProjectMetadata>(projectMetadata);
  }

  public static void DisplayProjectEnvironmentValues(Action<string> standardOutWriteLine,
    Action<ConsoleColor?, string> standardOutWrite,
    IReadOnlyDictionary<ProjectEnvironmentVariable, string> environmentVariableDisplayValues)
  {
    standardOutWriteLine(obj: "CI Environment:");
    if (environmentVariableDisplayValues.Any())
    {
      WriteEnvironmentVariables(environmentVariableDisplayValues);
    }
    else
    {
      standardOutWriteLine(obj: "  No CI environment variables defined.");
    }

    return;

    void WriteEnvironmentVariables(IReadOnlyDictionary<ProjectEnvironmentVariable, string> environmentDisplay)
    {
      int width = environmentDisplay.Keys.Max(value => value.Name.Length) + 1;
      foreach ((ProjectEnvironmentVariable key, string value) in environmentDisplay.OrderBy(kvp => kvp.Key.Name))
      {
        bool isPopulated = value != string.Empty;
        ConsoleColor? nameColor = key.Required && !isPopulated ? ConsoleColor.Red : null;
        ConsoleColor? valueColor = key.Secret && isPopulated ? ConsoleColor.Yellow :
          isPopulated ? ConsoleColor.Blue : ConsoleColor.DarkGray;
        string valueOrDefault = isPopulated || string.IsNullOrWhiteSpace(key.DefaultValue)
          ? value // When the value is populated or no default exists, display value. 
          // If there is a default, but no value, and it's a secret, show the secret placeholder, else the default. 
          : $"<{(key.Secret ? SecretString : key.DefaultValue)}>";

        standardOutWrite(nameColor, $"  {key.Name.PadRight(width, paddingChar: ' ')}");
        standardOutWrite(arg1: null, arg2: ": ");
        standardOutWrite(valueColor, valueOrDefault);
        standardOutWrite(arg1: null, Environment.NewLine);
      }
    }
  }
}
