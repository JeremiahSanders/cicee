using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cicee.CiEnv;
using LanguageExt;
using LanguageExt.Common;

namespace Cicee.Commands.Init
{
  public static class InitHandling
  {
    public static Result<InitRequest> TryCreateRequest(InitDependencies dependencies, string projectRoot, string? image,
      bool overwrite)
    {
      return new(new InitRequest(projectRoot, image, overwrite));
    }

    public static async Task<Result<InitRequest>> TryHandleRequest(InitDependencies dependencies, InitRequest request)
    {
      dependencies.WriteInformation("Initializing project...\n");
      return await Validation.ValidateRequestExecution(dependencies, request)
        .TapLeft(exception =>
          dependencies.WriteInformation(
            $"Request cannot be completed.\nError: {exception.GetType()}\nMessage: {exception.Message}"))
        .BindAsync(async validatedRequest =>
          (await TryWriteFiles(dependencies, validatedRequest))
          .TapLeft(exception =>
            dependencies.WriteInformation(
              $"Failed to write files.\nError: {exception.GetType()}\nMessage: {exception.Message}"
            )
          )
          .Tap(results =>
          {
            dependencies.WriteInformation("Initialization complete.\n\nFiles:");
            foreach (var result in results)
            {
              dependencies.WriteInformation(
                $"  {(result.Written ? "Copied " : "Skipped")} {result.Request.DestinationPath}");
            }
          })
          .Map(_ => validatedRequest)
        );
    }

    private static Task<Result<IReadOnlyCollection<FileCopyResult>>> TryWriteFiles(InitDependencies dependencies,
      InitRequest validatedRequest)
    {
      IReadOnlyDictionary<string, string> templateValues = new Dictionary<string, string>
      {
        {"image", validatedRequest.Image ?? string.Empty}
      };

      IReadOnlyCollection<FileCopyRequest> GetFiles()
      {
        var initTemplatesPath = dependencies.GetInitTemplatesDirectoryPath();
        var ciPath = dependencies.CombinePath(validatedRequest.ProjectRoot, Conventions.CiDirectoryName);
        var dockerfile = string.IsNullOrWhiteSpace(validatedRequest.Image)
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
          dockerfile, new FileCopyRequest(
            dependencies.CombinePath(initTemplatesPath, "example.ci.env"),
            dependencies.CombinePath(ciPath, "example.ci.env")
          ),
          new FileCopyRequest(
            dependencies.CombinePath(initTemplatesPath, "docker-compose.ci.dependencies.yml"),
            dependencies.CombinePath(validatedRequest.ProjectRoot, "docker-compose.ci.dependencies.yml")
          ),
          new FileCopyRequest(
            dependencies.CombinePath(initTemplatesPath, "docker-compose.ci.project.yml"),
            dependencies.CombinePath(validatedRequest.ProjectRoot, "docker-compose.ci.project.yml")
          )
        };
      }

      Result<FileCopyResult> TryCopyFile(FileCopyRequest fileCopyRequest)
      {
        return dependencies.DoesFileExist(fileCopyRequest.SourcePath)
          .Bind(exists =>
            exists
              ? dependencies.DoesFileExist(fileCopyRequest.DestinationPath)
              : new Result<bool>(
                new FileNotFoundException($"Failed to find template file '{fileCopyRequest.SourcePath}'.")
              )
          )
          .Bind(destinationExists =>
            !destinationExists || validatedRequest.OverwriteFiles
              ? dependencies.CopyTemplateToPath(fileCopyRequest, templateValues)
                .Map(savedRequest => new FileCopyResult(savedRequest, Written: true))
              : new Result<FileCopyResult>(new FileCopyResult(fileCopyRequest, Written: false))
          );
      }

      return GetFiles()
        .Fold(
          new Result<IEnumerable<FileCopyResult>>(Array.Empty<FileCopyResult>()),
          (lastResult, request) =>
            lastResult.Bind(fileCopyResults =>
              TryCopyFile(request)
                .Map(fileCopyResults.Append)
            )
        )
        .Map(results => (IReadOnlyCollection<FileCopyResult>)results.ToList())
        .AsTask();
    }
  }
}
