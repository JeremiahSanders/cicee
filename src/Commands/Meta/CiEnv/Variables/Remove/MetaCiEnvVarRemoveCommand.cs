using System.CommandLine;
using Cicee.Dependencies;

namespace Cicee.Commands.Meta.CiEnv.Variables.Remove;

public static class MetaCiEnvVarRemoveCommand
{
  public const string CommandName = "remove";
  public const string CommandDescription = "Removes a project CI environment variable.";
  public const string CommandAlias = "rm";

  public static Command Create(CommandDependencies dependencies)
  {
    var projectMetadata = ProjectMetadataOption.Create(dependencies);
    var name = VariablesOptions.CreateNameRequired();
    var command = new Command(CommandName, CommandDescription) { projectMetadata, name };
    command.AddAlias(CommandAlias);
    command.SetHandler(MetaCiEnvVarRemoveEntrypoint.CreateHandler(dependencies), projectMetadata, name);

    return command;
  }
}
