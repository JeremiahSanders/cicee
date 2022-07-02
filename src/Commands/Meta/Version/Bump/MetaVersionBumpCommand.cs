using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace Cicee.Commands.Meta.Version.Bump;

public static class MetaVersionBumpCommand
{
  public const string CommandName = "bump";
  public const string CommandDescription = "Increments version in project metadata.";

  private static Option<SemVerIncrement> IncrementOption()
  {
    return new Option<SemVerIncrement>(new[] {"--increment", "-i"}, () => SemVerIncrement.Minor,
      "SemVer increment by which to modify version. E.g., 'Minor' would bump 2.3.1 to 2.4.0.") {IsRequired = true};
  }

  public static Command Create(CommandDependencies dependencies)
  {
    var projectMetadata = ProjectMetadataOption.Create(dependencies);
    var dryRun = DryRunOption.Create();
    var increment = IncrementOption();
    var command = new Command(CommandName, CommandDescription) {projectMetadata, dryRun, increment};
    command.SetHandler(MetaVersionBumpEntrypoint.CreateHandler(dependencies), projectMetadata, dryRun, increment);

    return command;
  }
}

public static class MetaVersionBumpEntrypoint
{
  public static Func<string, bool, SemVerIncrement, Task<int>> CreateHandler(CommandDependencies dependencies)
  {
    async Task<int> Handle(string projectMetadataPath, bool isDryRun, SemVerIncrement semVerIncrement)
    {
      return (await MetaVersionBumpHandling.Handle(dependencies, projectMetadataPath, isDryRun, semVerIncrement))
        .TapSuccess(dependencies.StandardOutWriteLine)
        .TapFailure(exception =>
        {
          dependencies.StandardErrorWriteLine(exception.ToExecutionFailureMessage());
        })
        .ToExitCode();
    }

    return Handle;
  }
}
