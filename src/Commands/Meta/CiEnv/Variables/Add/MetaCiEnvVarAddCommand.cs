using System.CommandLine;
using System.CommandLine.Parsing;
using Cicee.Dependencies;

namespace Cicee.Commands.Meta.CiEnv.Variables.Add;

public static class MetaCiEnvVarAddCommand
{
  public const string CommandName = "add";
  public const string CommandDescription = "Adds a new project CI environment variable.";

  public static Command Create(CommandDependencies dependencies)
  {
    var projectMetadata = ProjectMetadataOption.Create(dependencies);
    var name = VariablesOptions.CreateNameRequired();
    var description = CreateDescriptionOption();
    var required = CreateRequiredOption();
    var secret = CreateSecretOption();
    var defaultValue = VariablesOptions.CreateDefaultValueOption();
    var command = new Command(CommandName, CommandDescription)
    {
      projectMetadata,
      name,
      description,
      required,
      secret,
      defaultValue
    };
    command.SetHandler(MetaCiEnvVarAddEntrypoint.CreateHandler(dependencies), projectMetadata, name,
      description, required, secret, defaultValue);

    return command;
  }


  private static Option<bool> CreateSecretOption()
  {
    const string OptionName = "--secret";
    const string OptionShort = "-s";
    const string OptionDescription = "Is this environment variable secret?";
    return new Option<bool>(new[] { OptionName, OptionShort }, () => false, OptionDescription);
  }

  private static Option<bool> CreateRequiredOption()
  {
    const string OptionName = "--required";
    const string OptionShort = "-r";
    const string OptionDescription = "Is this environment variable required?";
    return new Option<bool>(new[] { OptionName, OptionShort }, () => false, OptionDescription);
  }

  private static Option<string> CreateDescriptionOption()
  {
    const string OptionName = "--description";
    const string OptionShort = "-d";
    const string OptionDescription = "Environment variable description.";
    return new Option<string>(new[] { OptionName, OptionShort }, () => string.Empty, OptionDescription);
  }
}
