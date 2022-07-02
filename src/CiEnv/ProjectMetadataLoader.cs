using System;
using System.Collections.Generic;
using Cicee.Commands;
using LanguageExt.Common;

namespace Cicee.CiEnv;

public static class ProjectMetadataLoader
{
  public static readonly string DefaultMetadataFileName = "project-metadata.json";

  public static readonly IReadOnlyList<string> RelativeProjectMetadataNames =
    new[] {DefaultMetadataFileName, ".project-metadata.json", "package.json"};

  public static string CreateDefaultMetadataFileName(
    Func<string, string, string> combinePath,
    string projectRoot
  )
  {
    return combinePath(projectRoot, DefaultMetadataFileName);
  }

  public static Result<(string FilePath, ProjectMetadata ProjectMetadata)> TryFindProjectMetadata(
    Func<string, Result<string>> ensureDirectoryExists,
    Func<string, Result<string>> ensureFileExists,
    Func<string, Result<string>> tryLoadFileString,
    Func<string, string, string> combinePath,
    string projectRoot)
  {
    return
      ensureDirectoryExists(projectRoot)
        .Bind(_ =>
          RelativeProjectMetadataNames
            .Fold(
              new Result<(string FilePath, ProjectMetadata ProjectMetadata)>(new BadRequestException(
                $"Failed to find a suitable metadata file. Add a {DefaultMetadataFileName} file in the project root.")),
              (lastResult, filePath) => lastResult
                .BindLeft(_ =>
                  TryLoadFromFile(ensureFileExists, tryLoadFileString, combinePath(projectRoot, filePath))
                    .Map(metadata =>
                      (combinePath(projectRoot, filePath), metadata)
                    )
                )
            )
        );
  }

  public static Result<string> InferProjectMetadataPath(
    Func<string, Result<string>> ensureDirectoryExists,
    Func<string, Result<string>> ensureFileExists,
    Func<string, Result<string>> tryLoadFileString,
    Func<string, string, string> combinePath,
    Func<string, Result<string>> tryGetParentDirectory,
    Func<Result<string>> tryGetCurrentDirectory
  )
  {
    return tryGetCurrentDirectory()
      .Bind(initialDirectory =>
        TryInferProjectMetadataFromDirectories(
          ensureDirectoryExists,
          ensureFileExists,
          tryLoadFileString,
          combinePath,
          tryGetParentDirectory,
          initialDirectory
        )
      );
  }

  public static Result<string> TryInferProjectMetadataFromDirectories(
    Func<string, Result<string>> ensureDirectoryExists,
    Func<string, Result<string>> ensureFileExists,
    Func<string, Result<string>> tryLoadFileString,
    Func<string, string, string> combinePath,
    Func<string, Result<string>> tryGetParentDirectory,
    string initialDirectory
  )
  {
    return TryFindProjectMetadata(ensureDirectoryExists, ensureFileExists, tryLoadFileString,
        combinePath, initialDirectory)
      .Map(tuple => tuple.FilePath)
      .Match(
        value => new Result<string>(value),
        _ =>
          tryGetParentDirectory(initialDirectory)
            .Bind(parentDirectory => parentDirectory == initialDirectory
              ? new Result<string>(
                new InvalidOperationException("Metadata not found and no parent directories available."))
              : new Result<string>(parentDirectory))
            .Bind(parentDirectory =>
              TryInferProjectMetadataFromDirectories(
                ensureDirectoryExists,
                ensureFileExists,
                tryLoadFileString,
                combinePath,
                tryGetParentDirectory,
                parentDirectory
              )
            )
      );
  }

  public static Result<ProjectMetadata> TryLoadFromFile(
    Func<string, Result<string>> ensureFileExists,
    Func<string, Result<string>> tryLoadFileString,
    string filePath
  )
  {
    return
      ensureFileExists(filePath)
        .Bind(validatedFile => tryLoadFileString(validatedFile)
          .MapLeft(loadingFailure =>
            new BadRequestException("Failed to load project metadata.", loadingFailure))
        )
        .Bind(content =>
          Json.TryDeserialize<ProjectMetadata>(content)
            .MapLeft(deserializationFailure =>
              new BadRequestException("Failed to deserialize project metadata.", deserializationFailure)
            )
        );
  }


  public static ProjectMetadata InferProjectMetadata(CommandDependencies dependencies, string validatedProjectRoot)
  {
    return new ProjectMetadata
    {
      Name = InferProjectName(dependencies, validatedProjectRoot),
      Version = "0.0.0",
      Title = InferProjectTitle(dependencies, validatedProjectRoot)
    };
  }

  private static string InferProjectTitle(CommandDependencies dependencies, string validatedProjectRoot)
  {
    return dependencies.GetFileName(validatedProjectRoot);
  }

  private static string InferProjectName(CommandDependencies dependencies, string validatedProjectRoot)
  {
    return dependencies.GetFileName(validatedProjectRoot).ToKebabCase();
  }
}
