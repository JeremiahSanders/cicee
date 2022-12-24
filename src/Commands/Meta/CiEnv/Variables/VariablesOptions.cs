using System.CommandLine;

namespace Cicee.Commands.Meta.CiEnv.Variables;

internal static class VariablesOptions
{
  public static Option<string?> CreateDefaultValueOption()
  {
    const string OptionVerbose = "--defaultValue";
    const string OptionName = "--default";
    const string OptionShort = "-v";
    const string OptionDescription = "Default environment variable value.";
    return new Option<string?>(new[] { OptionVerbose, OptionName, OptionShort }, () => null, OptionDescription);
  }

  public static Option<string?> CreateNameOption(bool isRequired = true, string? description = null)
  {
    const string OptionName = "--name";
    const string OptionShort = "-n";
    const string OptionDescription = "Environment variable name.";
    var option =
      new Option<string?>(new[] { OptionName, OptionShort }, () => null, description ?? OptionDescription)
      {
        IsRequired = true
      };

    if (isRequired)
    {
      option.AddValidator(result =>
      {
        if (string.IsNullOrWhiteSpace(result.GetValueOrDefault<string?>()))
        {
          result.ErrorMessage = "Name is required.";
        }
      });
    }

    return option;
  }
}
