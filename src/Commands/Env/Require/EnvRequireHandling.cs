using System.Threading.Tasks;
using Cicee.CiEnv;
using LanguageExt;
using LanguageExt.Common;

namespace Cicee.Commands.Env.Require
{
  public static class EnvRequireHandling
  {
    public static Task<Result<EnvRequireResult>> TryHandleAsync(CommandDependencies dependencies,
      EnvRequireRequest envRequireRequest)
    {
      return TryValidateRequest(dependencies, envRequireRequest)
        .Bind(context => ProjectEnvironmentHelpers
          .ValidateEnvironment(dependencies.GetEnvironmentVariables, context.ProjectMetadata).Map(_ => context))
        .Tap(context =>
        {
          dependencies.StandardOutWriteLine("Environment validation succeeded.");
          dependencies.StandardOutWriteLine($"  Metadata file: {context.FilePath}");
        })
        .TapLeft(exception =>
        {
          dependencies.StandardErrorWriteLine("Environment validation failed.");
          dependencies.StandardErrorWriteLine($"  Reason: {exception.Message}");
        })
        .Map(context => new EnvRequireResult(context.FilePath, context.ProjectMetadata))
        .AsTask();
    }

    private static Result<EnvRequireRequestContext> TryValidateRequest(CommandDependencies dependencies,
      EnvRequireRequest envRequireRequest)
    {
      if (envRequireRequest.ProjectMetadataFile != null)
      {
        return ProjectMetadataLoader
          .TryLoadFromFile(dependencies.EnsureFileExists, dependencies.TryLoadFileString,
            envRequireRequest.ProjectMetadataFile)
          .Map(metadata =>
            new EnvRequireRequestContext(envRequireRequest.ProjectMetadataFile, metadata)
          );
      }

      if (envRequireRequest.ProjectRoot != null)
      {
        return ProjectMetadataLoader.TryFindProjectMetadata(dependencies.EnsureDirectoryExists,
            dependencies.EnsureFileExists,
            dependencies.TryLoadFileString, dependencies.CombinePath, envRequireRequest.ProjectRoot)
          .Map(kvp => new EnvRequireRequestContext(kvp.FilePath, kvp.ProjectMetadata));
      }

      return new Result<EnvRequireRequestContext>(
        new BadRequestException("Either a project metadata file or project root must be provided.")
      );
    }
  }
}
