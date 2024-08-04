using System.CommandLine;

using Cicee.Commands.Meta.CiEnv.Variables;
using Cicee.Dependencies;

namespace Cicee.Commands.Meta.CiEnv;

public static class CiEnvironmentCommand
{
  public const string CommandName = "cienvironment";
  public const string CommandDescription = "Commands related to the project's CI environment.";
  public const string CommandAlias = "cienv";

  public static Command Create(CommandDependencies dependencies)
  {
    Command command = new(CommandName, CommandDescription);
    command.AddAlias(CommandAlias);

    command.AddCommand(VariablesCommand.Create(dependencies));

    return command;
  }
}
