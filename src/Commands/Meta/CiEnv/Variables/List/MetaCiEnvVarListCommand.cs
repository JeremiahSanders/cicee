using System.CommandLine;

using Cicee.Dependencies;

namespace Cicee.Commands.Meta.CiEnv.Variables.List;

public static class MetaCiEnvVarListCommand
{
  public const string CommandName = "list";
  public const string CommandDescription = "Lists the project's CI environment variables.";
  public const string CommandAlias = "ls";

  public static Command Create(CommandDependencies dependencies)
  {
    Option<string> projectMetadata = ProjectMetadataOption.Create(dependencies);
    Option<string?> name = VariablesOptions.CreateNameOptional(
      description: "Environment variable name 'contains' filter, case insensitive."
    );
    Command command = new(CommandName, CommandDescription)
    {
      projectMetadata, name
    };
    command.AddAlias(CommandAlias);

    command.SetHandler(MetaCiEnvVarListEntrypoint.CreateHandler(dependencies), projectMetadata, name);

    return command;
  }
}
