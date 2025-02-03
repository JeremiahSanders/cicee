using System.CommandLine;

using Cicee.Commands.Meta.CiEnv.Variables.Add;
using Cicee.Commands.Meta.CiEnv.Variables.List;
using Cicee.Commands.Meta.CiEnv.Variables.Remove;
using Cicee.Commands.Meta.CiEnv.Variables.Update;
using Cicee.Dependencies;

namespace Cicee.Commands.Meta.CiEnv.Variables;

public static class VariablesCommand
{
  public const string CommandName = "variables";
  public const string CommandDescription = "Commands related to the project's CI environment variables.";
  public const string CommandAlias = "var";

  public static Command Create(ICommandDependencies dependencies)
  {
    Command command = new(CommandName, CommandDescription);
    command.AddAlias(CommandAlias);

    command.AddCommand(MetaCiEnvVarAddCommand.Create(dependencies));
    command.AddCommand(MetaCiEnvVarRemoveCommand.Create(dependencies));
    command.AddCommand(MetaCiEnvVarUpdateCommand.Create(dependencies));
    command.AddCommand(MetaCiEnvVarListCommand.Create(dependencies));

    return command;
  }
}
