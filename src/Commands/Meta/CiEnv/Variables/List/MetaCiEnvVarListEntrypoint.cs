using System;
using System.Linq;
using System.Threading.Tasks;
using Cicee.CiEnv;
using Cicee.Dependencies;
using LanguageExt;

namespace Cicee.Commands.Meta.CiEnv.Variables.List;

public static class MetaCiEnvVarListEntrypoint
{
  public static Func<string, string?, Task<int>> CreateHandler(CommandDependencies dependencies)
  {
    Task<int> Handle(string projectMetadataPath, string? nameContains)
    {
      return MetaCiEnvVarListHandling.MetaCiEnvVarListRequest(dependencies, projectMetadataPath, nameContains)
        .TapSuccess(variables => WriteVariablesToStandardOut(dependencies, variables))
        .TapFailure(exception => dependencies.StandardErrorWriteLine(exception.ToExecutionFailureMessage()))
        .ToExitCode()
        .AsTask();
    }

    return Handle;
  }

  private static void WriteVariablesToStandardOut(CommandDependencies dependencies,
    ProjectEnvironmentVariable[] variables)
  {
    const ConsoleColor requiredSecret = ConsoleColor.Magenta;
    const ConsoleColor requiredPublic = ConsoleColor.DarkRed;
    const ConsoleColor optionalSecret = ConsoleColor.DarkCyan;
    var varLengths = variables.Select(variable => variable.Name.Length).OrderByDescending(Prelude.identity)
      .ToList();
    const int minPadding = 4; // Length of "name"
    const int maxPadding = 30;
    var longest = varLengths.FirstOrDefault();
    var paddingLength = Math.Clamp(longest, minPadding, maxPadding);

    dependencies.StandardOutWriteLine("CI Environment Variables");

    dependencies.StandardOutWrite(arg1: null, "Name".PadRight(paddingLength));
    dependencies.StandardOutWrite(arg1: null, "  Req Sec");
    if (variables.Any(variable => !string.IsNullOrWhiteSpace(variable.Description)))
    {
      dependencies.StandardOutWrite(arg1: null, " Description");
    }

    dependencies.StandardOutWrite(arg1: null, Environment.NewLine);

    foreach (var variable in variables.OrderBy(variable => variable.Name.ToUpperInvariant()))
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
      var requiredDisplay = variable.Required ? "*  " : "   ";
      var secretDisplay = variable.Secret ? "* " : "  ";
      dependencies.StandardOutWrite(arg1: null, "   "); // Gap between name and required
      dependencies.StandardOutWrite(variable.Required ? ConsoleColor.Red : null, requiredDisplay);
      dependencies.StandardOutWrite(arg1: null, " "); // Gap between required and secret
      dependencies.StandardOutWrite(variable.Secret ? ConsoleColor.DarkCyan : null, secretDisplay);
      dependencies.StandardOutWrite(arg1: null, $" {variable.Description}");
      if (!string.IsNullOrWhiteSpace(variable.DefaultValue))
      {
        var prefix = string.IsNullOrWhiteSpace(variable.Description) ? "" : " ";
        dependencies.StandardOutWrite(arg1: null, $"{prefix}(Default: ");
        var valueDisplay = variable.Secret ? ProjectEnvironmentHelpers.SecretString : variable.DefaultValue;
        dependencies.StandardOutWrite(arg1: null, valueDisplay);
        dependencies.StandardOutWrite(arg1: null, ")");
      }

      dependencies.StandardOutWrite(arg1: null, Environment.NewLine);
    }
  }
}
