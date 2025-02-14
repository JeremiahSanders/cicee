using System.Collections.Generic;

using Cicee.Dependencies;

namespace Cicee.Commands.Exec.Handling;

public static class IoEnvironment
{
  public static IReadOnlyDictionary<string, string> GetExecEnvironment(
    CommandDependencies dependencies,
    ExecRequestContext context,
    bool forcePathsToLinux
  )
  {
    Dictionary<string, string> environment = new()
    {
      [HandlingConstants.CiExecContext] = ConditionalConvert(
        dependencies.CombinePath(context.ProjectRoot, HandlingConstants.CiDirectoryName)
      ),
      [HandlingConstants.ProjectName] = context.ProjectMetadata.Name,
      [HandlingConstants.ProjectRoot] = ConditionalConvert(context.ProjectRoot),
      [HandlingConstants.LibRoot] = ConditionalConvert(dependencies.GetLibraryRootPath()),
      // Explicit empty string default applied to prevent Docker Compose reporting that it is defaulting to empty strings.
      [HandlingConstants.CiCommand] = context.Command ?? string.Empty,
      [HandlingConstants.CiEntrypoint] = context.Entrypoint ?? string.Empty
    };

    ConditionallyAdd(HandlingConstants.CiExecImage, context.Image);

    return environment;

    void ConditionallyAdd(string key, string? possibleValue)
    {
      if (!string.IsNullOrWhiteSpace(possibleValue))
      {
        environment[key] = possibleValue!;
      }
    }

    string ConditionalConvert(string path)
    {
      return forcePathsToLinux ? Io.NormalizeToLinuxPath(path) : path;
    }
  }
}
