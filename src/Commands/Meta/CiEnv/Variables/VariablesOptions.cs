using System.CommandLine;
using System.CommandLine.Parsing;

namespace Cicee.Commands.Meta.CiEnv.Variables;

internal static class VariablesOptions
{
  private const string NameOptionName = "--name";
  private const string NameOptionShort = "-n";
  private const string NameOptionDescription = "Environment variable name.";

  private static string[] CreateNameAliases()
  {
    return new[] { NameOptionName, NameOptionShort };
  }

  public static Option<string?> CreateDefaultValueOption()
  {
    const string OptionVerbose = "--defaultValue";
    const string OptionName = "--default";
    const string OptionShort = "-v";
    const string OptionDescription = "Default environment variable value.";
    return new Option<string?>(new[] { OptionVerbose, OptionName, OptionShort }, () => null, OptionDescription);
  }

  private static void ValidateRequiredName(OptionResult result)
  {
    if (string.IsNullOrWhiteSpace(result.GetValueOrDefault<string?>()))
    {
      result.ErrorMessage = "Name is required.";
    }
  }

  public static Option<string> CreateNameRequired(string? description = null)
  {
    var option =
      new Option<string>(CreateNameAliases(), description ?? NameOptionDescription) { IsRequired = true };
    option.AddValidator(ValidateRequiredName);
    return option;
  }

  public static Option<string?> CreateNameOptional(string? description = null)
  {
    return new Option<string?>(CreateNameAliases(), () => null,
      description ?? NameOptionDescription) { IsRequired = false };
  }
}
