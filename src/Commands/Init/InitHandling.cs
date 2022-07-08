using System.Collections.Generic;
using System.Threading.Tasks;
using Cicee.CiEnv;
using Cicee.Dependencies;
using LanguageExt.Common;

namespace Cicee.Commands.Init
{
  public static class InitHandling
  {
    public static Result<InitRequest> TryCreateRequest(CommandDependencies dependencies, string projectRoot,
      string? image,
      bool overwrite)
    {
      return new(new InitRequest(projectRoot, image, overwrite));
    }

    private static IReadOnlyCollection<FileCopyRequest> GetFiles(CommandDependencies dependencies, InitRequest request)
    {
      var initTemplatesPath = dependencies.GetInitTemplatesDirectoryPath();
      var ciPath = dependencies.CombinePath(request.ProjectRoot, Conventions.CiDirectoryName);
      var dockerfile = string.IsNullOrWhiteSpace(request.Image)
        ? new FileCopyRequest(
          dependencies.CombinePath(initTemplatesPath, "Default.Dockerfile"),
          dependencies.CombinePath(ciPath, "Dockerfile")
        )
        : new FileCopyRequest(
          dependencies.CombinePath(initTemplatesPath, "Template.Dockerfile"),
          dependencies.CombinePath(ciPath, "Dockerfile")
        );
      return new[]
      {
        dockerfile,
        new FileCopyRequest(
          dependencies.CombinePath(initTemplatesPath, "docker-compose.dependencies.yml"),
          dependencies.CombinePath(ciPath, "docker-compose.dependencies.yml")
        ),
        new FileCopyRequest(
          dependencies.CombinePath(initTemplatesPath, "docker-compose.project.yml"),
          dependencies.CombinePath(ciPath, "docker-compose.project.yml")
        )
      };
    }

    private static IReadOnlyDictionary<string, string> GetTemplateValues(InitRequest request)
    {
      return new Dictionary<string, string> { { "image", request.Image ?? string.Empty } };
    }

    public static async Task<Result<InitRequest>> TryHandleRequest(CommandDependencies dependencies,
      InitRequest request)
    {
      dependencies.StandardOutWriteLine("Initializing project...\n");
      return await Validation.ValidateRequestExecution(dependencies, request)
        .TapFailure(exception =>
          dependencies.StandardOutWriteLine(
            $"Request cannot be completed.\nError: {exception.GetType()}\nMessage: {exception.Message}"))
        .BindAsync(async validatedRequest =>
          (await FileCopyHelpers.TryWriteFiles(
            dependencies,
            GetFiles(dependencies, validatedRequest),
            GetTemplateValues(request),
            validatedRequest.OverwriteFiles
          ))
          .TapFailure(exception =>
            dependencies.StandardOutWriteLine(
              $"Failed to write files.\nError: {exception.GetType()}\nMessage: {exception.Message}"
            )
          )
          .TapSuccess(results =>
          {
            dependencies.StandardOutWriteLine("Initialization complete.\n\nFiles:");
            foreach (var result in results)
            {
              dependencies.StandardOutWriteLine(
                $"  {(result.Written ? "Copied " : "Skipped")} {result.Request.DestinationPath}");
            }
          })
          .Map(_ => validatedRequest)
        );
    }
  }
}
