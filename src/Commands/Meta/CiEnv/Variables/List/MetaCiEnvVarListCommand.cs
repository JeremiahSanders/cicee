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
    var projectMetadata = ProjectMetadataOption.Create(dependencies);
    var name = VariablesOptions.CreateNameOption(isRequired: false, "Environment variable name 'contains' filter, case insensitive.");
    var command = new Command(CommandName, CommandDescription) { projectMetadata, name };
    command.AddAlias(CommandAlias);

    command.SetHandler(MetaCiEnvVarListEntrypoint.CreateHandler(dependencies), projectMetadata, name);

    return command;
  }
}
