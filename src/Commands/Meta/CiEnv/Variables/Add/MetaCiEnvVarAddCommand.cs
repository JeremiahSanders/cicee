using System.CommandLine;

using Cicee.Dependencies;

namespace Cicee.Commands.Meta.CiEnv.Variables.Add;

public static class MetaCiEnvVarAddCommand
{
  public const string CommandName = "add";
  public const string CommandDescription = "Adds a new project CI environment variable.";

  public static Command Create(CommandDependencies dependencies)
  {
    Option<string> projectMetadata = ProjectMetadataOption.Create(dependencies);
    Option<string> name = VariablesOptions.CreateNameRequired();
    Option<string> description = CreateDescriptionOption();
    Option<bool> required = CreateRequiredOption();
    Option<bool> secret = CreateSecretOption();
    Option<string?> defaultValue = VariablesOptions.CreateDefaultValueOption();
    Command command = new(CommandName, CommandDescription)
    {
      projectMetadata,
      name,
      description,
      required,
      secret,
      defaultValue
    };
    command.SetHandler(
      MetaCiEnvVarAddEntrypoint.CreateHandler(dependencies),
      projectMetadata,
      name,
      description,
      required,
      secret,
      defaultValue
    );

    return command;
  }


  private static Option<bool> CreateSecretOption()
  {
    const string optionName = "--secret";
    const string optionShort = "-s";
    const string optionDescription = "Is this environment variable secret?";
    return new Option<bool>(
      new[]
      {
        optionName,
        optionShort
      },
      () => false,
      optionDescription
    );
  }

  private static Option<bool> CreateRequiredOption()
  {
    const string optionName = "--required";
    const string optionShort = "-r";
    const string optionDescription = "Is this environment variable required?";
    return new Option<bool>(
      new[]
      {
        optionName,
        optionShort
      },
      () => false,
      optionDescription
    );
  }

  private static Option<string> CreateDescriptionOption()
  {
    const string optionName = "--description";
    const string optionShort = "-d";
    const string optionDescription = "Environment variable description.";
    return new Option<string>(
      new[]
      {
        optionName,
        optionShort
      },
      () => string.Empty,
      optionDescription
    );
  }
}
