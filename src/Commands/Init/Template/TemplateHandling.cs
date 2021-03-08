using System.Collections.Generic;
using System.Threading.Tasks;
using Cicee.CiEnv;
using LanguageExt.Common;

namespace Cicee.Commands.Init.Template
{
  public static class TemplateHandling
  {
    internal static Result<TemplateRequest> TryCreateRequest(InitDependencies dependencies, string projectRoot,
      bool force)
    {
      return new(new TemplateRequest(projectRoot, force));
    }

    private static IReadOnlyCollection<FileCopyRequest> GetFiles(InitDependencies dependencies,
      TemplateRequest request)
    {
      var templatesPath = dependencies.CombinePath(dependencies.GetInitTemplatesDirectoryPath(), "bin");
      var ciPath = dependencies.CombinePath(request.ProjectRoot, Conventions.CiDirectoryName);
      var ciBinPath = dependencies.CombinePath(ciPath, Conventions.CiBinDirectoryName);
      return new[]
      {
        new FileCopyRequest(
          dependencies.CombinePath(templatesPath, "ci-actions.sh"),
          dependencies.CombinePath(ciBinPath, "ci-actions.sh")
        ),
        new FileCopyRequest(
          dependencies.CombinePath(templatesPath, "ci-workflows.sh"),
          dependencies.CombinePath(ciBinPath, "ci-workflows.sh")
        ),
        new FileCopyRequest(
          dependencies.CombinePath(templatesPath, "compose.sh"),
          dependencies.CombinePath(ciBinPath, "compose.sh")
        ),
        new FileCopyRequest(
          dependencies.CombinePath(templatesPath, "publish.sh"),
          dependencies.CombinePath(ciBinPath, "publish.sh")
        ),
        new FileCopyRequest(
          dependencies.CombinePath(templatesPath, "validate.sh"),
          dependencies.CombinePath(ciBinPath, "validate.sh")
        )
      };
    }

    private static IReadOnlyDictionary<string, string> GetTemplateValues(TemplateRequest request)
    {
      return new Dictionary<string, string>();
    }

    public static async Task<Result<TemplateResult>> TryHandleRequest(InitDependencies dependencies,
      TemplateRequest request)
    {
      dependencies.WriteInformation("Initializing project...\n");
      return await Validation.ValidateRequestExecution(dependencies, request)
        .TapLeft(exception =>
          dependencies.WriteInformation(
            $"Request cannot be completed.\nError: {exception.GetType()}\nMessage: {exception.Message}"))
        .BindAsync(async validatedRequest =>
          (await FileCopyHelpers.TryWriteFiles(
            dependencies,
            GetFiles(dependencies, validatedRequest),
            GetTemplateValues(request),
            validatedRequest.OverwriteFiles
          ))
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
          .Map(_ => new TemplateResult(validatedRequest.ProjectRoot, validatedRequest.OverwriteFiles))
          .Tap(result => DisplayNextSteps(dependencies, result))
        );
    }

    private static void DisplayNextSteps(InitDependencies dependencies, TemplateResult result)
    {
      var projectCiBin = dependencies.CombinePath(
        dependencies.CombinePath(result.ProjectRoot, "ci"),
        "bin"
      );
      var workflowsScriptLocation = dependencies.CombinePath(projectCiBin, "ci-workflows.sh");
      var nextSteps = $@"
Continuous integration scripts initialized successfully.

The following workflow entrypoints were initialized in {projectCiBin}:

 * validate.sh - Validate the project code.
   Expected use: Execute during pull request review to provide static code
                 analysis and run tests.

 * compose.sh  - Builds the project distributable artifacts.
                 E.g., NuGet packages, Docker images, zip archives
   Expected use: Execute locally to create distributable artifacts for local
                 use or manual validation. 

 * publish.sh  - Builds and publishes the project distributable artifacts.
                 E.g., push a Docker image, publish an NPM package
   Expected use: Execute after a project repository merge creates a new project
                 release. E.g., after a merge to 'main' or 'trunk'

Next steps:
  * Add execute permission to all initialized scripts.
    If using macOS or Linux:
      Run the following from your shell (from {result.ProjectRoot}):
      chmod +x ci/bin/*.sh
    If using Windows:
      After adding the scripts to Git, run the following from Git Bash (from {result.ProjectRoot}):
      git update-index --chmod=+x ci/bin/*.sh
  * Update {workflowsScriptLocation}.
    Setup the continuous integration processes the project needs.
";

      dependencies.WriteInformation(nextSteps);
    }
  }
}
