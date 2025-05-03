using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Cicee.CiEnv;
using Cicee.Dependencies;

using LanguageExt;

namespace Cicee.Commands.Meta.CiEnv.Variables.List;

public static class MetaCiEnvVarListEntrypoint
{
  public static Func<string, string?, Task<int>> CreateHandler(ICommandDependencies dependencies)
  {
    return Handle;

    Task<int> Handle(string projectMetadataPath, string? nameContains)
    {
      return MetaCiEnvVarListHandling
        .MetaCiEnvVarListRequest(dependencies, projectMetadataPath, nameContains)
        .TapSuccess(variables => WriteVariablesToStandardOut(dependencies, variables))
        .TapFailure(
          exception => dependencies.StandardErrorWriteLine(exception.ToExecutionFailureMessage())
        )
        .ToExitCode()
        .AsTask();
    }
  }

  private static void WriteVariablesToStandardOut(
    ICommandDependencies dependencies,
    ProjectEnvironmentVariable[] variables)
  {
    const ConsoleColor requiredSecret = ConsoleColor.Magenta;
    const ConsoleColor requiredPublic = ConsoleColor.DarkRed;
    const ConsoleColor optionalSecret = ConsoleColor.DarkCyan;
    List<int> varLengths = variables
      .Select(variable => variable.Name.Length)
      .OrderByDescending(Prelude.identity)
      .ToList();
    const int minPadding = 4; // Length of "name"
    const int maxPadding = 30;
    int longest = varLengths.FirstOrDefault();
    int paddingLength = Math.Clamp(longest, minPadding, maxPadding);

    dependencies.StandardOutWriteLine(text: "CI Environment Variables");

    dependencies.StandardOutWrite(color: null, "Name".PadRight(paddingLength));
    dependencies.StandardOutWrite(color: null, text: "  Req Sec");
    if (variables.Any(variable => !string.IsNullOrWhiteSpace(variable.Description)))
    {
      dependencies.StandardOutWrite(color: null, text: " Description");
    }

    dependencies.StandardOutWrite(color: null, Environment.NewLine);

    foreach (ProjectEnvironmentVariable variable in variables.OrderBy(variable => variable.Name.ToUpperInvariant()))
    {
      ConsoleColor? color;
      if (variable.Required)
      {
        color = variable.Secret ? requiredSecret : requiredPublic;
      }
      else
      {
        if (variable.Secret)
        {
          color = optionalSecret;
        }
        else
        {
          color = null;
        }
      }

      dependencies.StandardOutWrite(color, variable.Name.PadRight(paddingLength));
      // Extra spaces apply column padding
      string requiredDisplay = variable.Required ? "*  " : "   ";
      string secretDisplay = variable.Secret ? "* " : "  ";
      dependencies.StandardOutWrite(color: null, text: "   "); // Gap between name and required
      dependencies.StandardOutWrite(variable.Required ? ConsoleColor.Red : null, requiredDisplay);
      dependencies.StandardOutWrite(color: null, text: " "); // Gap between required and secret
      dependencies.StandardOutWrite(variable.Secret ? ConsoleColor.DarkCyan : null, secretDisplay);
      dependencies.StandardOutWrite(color: null, $" {variable.Description}");
      if (!string.IsNullOrWhiteSpace(variable.DefaultValue))
      {
        string prefix = string.IsNullOrWhiteSpace(variable.Description) ? "" : " ";
        dependencies.StandardOutWrite(color: null, $"{prefix}(Default: ");
        string? valueDisplay = variable.Secret ? ProjectEnvironmentHelpers.SecretString : variable.DefaultValue;
        dependencies.StandardOutWrite(color: null, valueDisplay);
        dependencies.StandardOutWrite(color: null, text: ")");
      }

      dependencies.StandardOutWrite(color: null, Environment.NewLine);
    }
  }
}
