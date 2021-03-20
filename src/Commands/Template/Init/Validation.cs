using Cicee.CiEnv;
using LanguageExt.Common;

namespace Cicee.Commands.Template.Init
{
  public static class Validation
  {
    public static Result<TemplateInitContext> ValidateRequestExecution(CommandDependencies dependencies,
      TemplateInitRequest request)
    {
      return dependencies.EnsureDirectoryExists(request.ProjectRoot)
        .Map(validatedProjectRoot =>
          {
            var (metadataFilePath, projectMetadata) = ProjectMetadataLoader.TryFindProjectMetadata(
                dependencies.EnsureDirectoryExists,
                dependencies.EnsureFileExists, dependencies.TryLoadFileString, dependencies.CombinePath,
                validatedProjectRoot)
              .IfFail(_ => (
                  FilePath: dependencies.CombinePath(validatedProjectRoot,
                    ProjectMetadataLoader.DefaultMetadataFileName),
                  ProjectMetadata: ProjectMetadataLoader.InferProjectMetadata(dependencies, validatedProjectRoot)
                )
              );
            return new TemplateInitContext
            (validatedProjectRoot,
              request.OverwriteFiles,
              request.MetadataFile ?? metadataFilePath,
              projectMetadata
            );
          }
        );
    }
  }
}
