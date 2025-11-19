using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;

using Cicee.Dependencies;

namespace Cicee.Commands.Meta.Version.Set;

public static class MetaVersionSetCommand
{
  public const string CommandName = "set";
  public const string CommandDescription = "Sets version in project metadata.";

  private static Option<System.Version> IncrementOption()
  {
    return new Option<System.Version>(
      new[]
      {
        "--version",
        "-v"
      },
      description: "New version in SemVer 2.0 release format. E.g., '2.3.1'."
    )
    {
      IsRequired = true
    };
  }

  private static Option<System.Version?> CreateVersionOption()
  {
    return new Option<System.Version?>(
      new[]
      {
        "--version",
        "-v"
      },
      ParseArgument,
      isDefault: true,
      description: "New version in SemVer 2.0 release format. E.g., '2.3.1'."
    )
    {
      IsRequired = true
    };

    System.Version? ParseArgument(ArgumentResult result)
    {
      string initialTokenValue = result.Tokens.Count > 0 ? result.Tokens[index: 0].Value : string.Empty;
      string tokenValue = initialTokenValue;
      int countOfPeriods = tokenValue.Count(c => c == '.');

      if (tokenValue != string.Empty)
      {
        switch (countOfPeriods)
        {
          case 0:
            if (int.TryParse(tokenValue, out _))
            {
              tokenValue += ".0.0"; // Single integer value 
            }

            break;
          case 1:
            tokenValue += ".0"; // Two-field version value

            break;
        }
      }

      if (System.Version.TryParse(tokenValue, out System.Version? version))
      {
        return version;
      }

      result.ErrorMessage =
        $"Invalid version format '{initialTokenValue}'. Use complete Major.Minor.Patch, e.g., '2.3.1' or '4.0.0'.";

      return null;
    }
  }

  public static Command Create(ICommandDependencies dependencies)
  {
    Option<string> projectMetadata = ProjectMetadataOption.Create(dependencies);
    Option<bool> dryRun = DryRunOption.Create();
    Option<System.Version?> versionOption = CreateVersionOption();
    Command command = new(CommandName, CommandDescription)
    {
      projectMetadata, dryRun, versionOption
    };
    command.SetHandler(MetaVersionSetEntrypoint.CreateHandler(dependencies), projectMetadata, dryRun, versionOption);

    return command;
  }
}
