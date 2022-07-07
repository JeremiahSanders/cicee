using System.CommandLine;
using Cicee.Commands.Meta.Version.Bump;
using Cicee.Dependencies;

namespace Cicee.Commands.Meta.Version;

public static class MetaVersionCommand
{
  public const string CommandName = "version";
  public const string CommandDescription = "Gets version in project metadata.";

  public static Command Create(CommandDependencies dependencies)
  {
    var projectMetadata = ProjectMetadataOption.Create(dependencies);
    var command = new Command(CommandName, CommandDescription) {projectMetadata};
    command.SetHandler(MetaVersionEntrypoint.CreateHandler(dependencies), projectMetadata);

    command.AddCommand(MetaVersionBumpCommand.Create(dependencies));

    return command;
  }
}
