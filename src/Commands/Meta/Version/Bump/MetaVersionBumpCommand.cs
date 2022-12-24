using System.CommandLine;
using Cicee.Dependencies;

namespace Cicee.Commands.Meta.Version.Bump;

public static class MetaVersionBumpCommand
{
  public const string CommandName = "bump";
  public const string CommandDescription = "Increments version in project metadata.";

  private static Option<SemVerIncrement> IncrementOption()
  {
    return new Option<SemVerIncrement>(new[] { "--increment", "-i" }, () => SemVerIncrement.Minor,
      "SemVer increment by which to modify version. E.g., 'Minor' would bump 2.3.1 to 2.4.0.") { IsRequired = true };
  }

  public static Command Create(CommandDependencies dependencies)
  {
    var projectMetadata = ProjectMetadataOption.Create(dependencies);
    var dryRun = DryRunOption.Create();
    var increment = IncrementOption();
    var command = new Command(CommandName, CommandDescription) { projectMetadata, dryRun, increment };
    command.SetHandler(MetaVersionBumpEntrypoint.CreateHandler(dependencies), projectMetadata, dryRun, increment);

    return command;
  }
}
