using System.CommandLine;

using Cicee.Commands.Meta.Version.Bump;
using Cicee.Commands.Meta.Version.Set;
using Cicee.Dependencies;

namespace Cicee.Commands.Meta.Version;

public static class MetaVersionCommand
{
  public const string CommandName = "version";
  public const string CommandDescription = "Gets version in project metadata.";

  public static Command Create(CommandDependencies dependencies)
  {
    Option<string> projectMetadata = ProjectMetadataOption.Create(dependencies);
    Command command = new(CommandName, CommandDescription)
    {
      projectMetadata
    };
    command.SetHandler(MetaVersionEntrypoint.CreateHandler(dependencies), projectMetadata);

    command.AddCommand(MetaVersionBumpCommand.Create(dependencies));
    command.AddCommand(MetaVersionSetCommand.Create(dependencies));

    return command;
  }
}
