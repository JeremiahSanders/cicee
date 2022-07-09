using System;
using System.Threading.Tasks;
using Cicee.CiEnv;
using Cicee.Dependencies;
using LanguageExt;

namespace Cicee.Commands.Env.Display;

public static class EnvDisplayEntrypoint
{
  public static Func<string, Task<int>> CreateHandler(CommandDependencies dependencies)
  {
    return projectMetadataPath => EnvDisplayHandling.TryHandle(dependencies.EnsureFileExists,
        dependencies.TryLoadFileString,
        dependencies.GetEnvironmentVariables, projectMetadataPath)
      .TapSuccess(response =>
      {
        dependencies.StandardOutWriteLine($"Metadata: {response.ProjectMetadataPath}");
        dependencies.StandardOutWriteLine(string.Empty);
        ProjectEnvironmentHelpers.DisplayProjectEnvironmentValues(
          dependencies.StandardOutWriteLine,
          dependencies.StandardOutWrite,
          response.Environment
        );
      })
      .TapFailure(exception =>
      {
        dependencies.StandardErrorWriteLine(exception.ToExecutionFailureMessage());
      })
      .ToExitCode()
      .AsTask();
  }
}
