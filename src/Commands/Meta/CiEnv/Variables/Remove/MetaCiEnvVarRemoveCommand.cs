using System.CommandLine;

using Cicee.Dependencies;

namespace Cicee.Commands.Meta.CiEnv.Variables.Remove;

public static class MetaCiEnvVarRemoveCommand
{
  public const string CommandName = "remove";
  public const string CommandDescription = "Removes a project CI environment variable.";
  public const string CommandAlias = "rm";

  public static Command Create(ICommandDependencies dependencies)
  {
    Option<string> projectMetadata = ProjectMetadataOption.Create(dependencies);
    Option<string> name = VariablesOptions.CreateNameRequired();
    Command command = new(CommandName, CommandDescription)
    {
      projectMetadata, name
    };
    command.AddAlias(CommandAlias);
    command.SetHandler(MetaCiEnvVarRemoveEntrypoint.CreateHandler(dependencies), projectMetadata, name);

    return command;
  }
}
