using System.Collections.Generic;

using Cicee.Dependencies;

namespace Cicee.Commands.Exec.Handling;

public static class IoEnvironment
{
  public static IReadOnlyDictionary<string, string> GetExecEnvironment(CommandDependencies dependencies,
    ExecRequestContext context)
  {
    Dictionary<string, string> environment = new()
    {
      [HandlingConstants.CiExecContext] =
        Io.NormalizeToLinuxPath(dependencies.CombinePath(context.ProjectRoot, HandlingConstants.CiDirectoryName)),
      [HandlingConstants.ProjectName] = context.ProjectMetadata.Name,
      [HandlingConstants.ProjectRoot] = Io.NormalizeToLinuxPath(context.ProjectRoot),
      [HandlingConstants.LibRoot] = Io.NormalizeToLinuxPath(dependencies.GetLibraryRootPath())
    };

    ConditionallyAdd(HandlingConstants.CiCommand, context.Command);
    ConditionallyAdd(HandlingConstants.CiEntrypoint, context.Entrypoint);
    ConditionallyAdd(HandlingConstants.CiExecImage, context.Image);

    return environment;

    void ConditionallyAdd(string key, string? possibleValue)
    {
      if (!string.IsNullOrWhiteSpace(possibleValue))
      {
        environment[key] = possibleValue!;
      }
    }
  }
}
