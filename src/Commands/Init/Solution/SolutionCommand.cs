using System.CommandLine;
using Cicee.Commands.Init.Solution.Dotnet;
using Cicee.Dependencies;

namespace Cicee.Commands.Init.Solution;

/// <summary>
///   Command to initialize a project repository with a solution template.
/// </summary>
public static class SolutionCommand
{
  public static Command Create(CommandDependencies commandDependencies)
  {
    var command = new Command("solution", "Initialize a .NET solution template.");
    
    command.AddCommand(DotnetCommand.Create(commandDependencies));

    return command;
  }
}
