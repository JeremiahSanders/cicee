using System.Threading.Tasks;

using Cicee.CiEnv;
using Cicee.Dependencies;

using LanguageExt;
using LanguageExt.Common;

namespace Cicee.Commands.Env.Require;

public static class EnvRequireHandling
{
  public static Task<Result<EnvRequireResult>> TryHandleAsync(
    ICommandDependencies dependencies,
    EnvRequireRequest envRequireRequest)
  {
    return TryValidateRequest(dependencies, envRequireRequest)
      .Bind(
        context => ProjectEnvironmentHelpers
          .ValidateEnvironment(
            dependencies.GetEnvironmentVariables,
            context.ProjectMetadata
          )
          .Map(_ => context)
      )
      .TapSuccess(
        context =>
        {
          dependencies.StandardOutWriteLine(text: "Environment validation succeeded.");
          dependencies.StandardOutWriteLine($"  Metadata file: {context.FilePath}");
        }
      )
      .TapFailure(
        exception =>
        {
          dependencies.StandardErrorWriteLine(text: "Environment validation failed.");
          dependencies.StandardErrorWriteLine($"  Reason: {exception.Message}");
        }
      )
      .Map(context => new EnvRequireResult(context.FilePath, context.ProjectMetadata))
      .AsTask();
  }

  private static Result<EnvRequireRequestContext> TryValidateRequest(
    ICommandDependencies dependencies,
    EnvRequireRequest envRequireRequest)
  {
    if (envRequireRequest.ProjectMetadataFile != null)
    {
      return ProjectMetadataLoader
        .TryLoadFromFile(
          dependencies.EnsureFileExists,
          dependencies.TryLoadFileString,
          envRequireRequest.ProjectMetadataFile
        )
        .Map(metadata => new EnvRequireRequestContext(envRequireRequest.ProjectMetadataFile, metadata));
    }

    if (envRequireRequest.ProjectRoot != null)
    {
      return ProjectMetadataLoader
        .TryFindProjectMetadata(
          dependencies.EnsureDirectoryExists,
          dependencies.EnsureFileExists,
          dependencies.TryLoadFileString,
          dependencies.CombinePath,
          envRequireRequest.ProjectRoot
        )
        .Map(kvp => new EnvRequireRequestContext(kvp.FilePath, kvp.ProjectMetadata));
    }

    return new Result<EnvRequireRequestContext>(
      new BadRequestException(message: "Either a project metadata file or project root must be provided.")
    );
  }
}
