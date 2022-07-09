using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cicee.CiEnv;
using Cicee.Dependencies;
using LanguageExt.Common;

namespace Cicee.Commands.Template.Init;

public static class TemplateInitHandling
{
  private static IReadOnlyCollection<FileCopyRequest> GetFiles(CommandDependencies dependencies,
    TemplateInitContext request)
  {
    var templatesBinPath =
      dependencies.CombinePath(dependencies.GetInitTemplatesDirectoryPath(), Conventions.CiBinDirectoryName);
    var templatesLibExecPath =
      dependencies.CombinePath(dependencies.GetInitTemplatesDirectoryPath(), Conventions.CiLibExecDirectoryName);
    var templatesLibExecWorkflowsPath =
      dependencies.CombinePath(templatesLibExecPath, Conventions.CiLibExecWorkflowsDirectoryName);
    var ciPath = dependencies.CombinePath(request.ProjectRoot, Conventions.CiDirectoryName);
    var ciBinPath = dependencies.CombinePath(ciPath, Conventions.CiBinDirectoryName);
    var ciLibExecPath = dependencies.CombinePath(ciPath, Conventions.CiLibExecDirectoryName);
    var ciLibExecWorkflowsPath = dependencies.CombinePath(ciLibExecPath, Conventions.CiLibExecWorkflowsDirectoryName);
    return new[]
    {
      new FileCopyRequest(
        dependencies.CombinePath(templatesLibExecWorkflowsPath, "ci-compose.sh"),
        dependencies.CombinePath(ciLibExecWorkflowsPath, "ci-compose.sh")
      ),
      new FileCopyRequest(
        dependencies.CombinePath(templatesLibExecWorkflowsPath, "ci-publish.sh"),
        dependencies.CombinePath(ciLibExecWorkflowsPath, "ci-publish.sh")
      ),
      new FileCopyRequest(
        dependencies.CombinePath(templatesLibExecWorkflowsPath, "ci-validate.sh"),
        dependencies.CombinePath(ciLibExecWorkflowsPath, "ci-validate.sh")
      ),
      new FileCopyRequest(
        dependencies.CombinePath(templatesBinPath, "publish.sh"),
        dependencies.CombinePath(ciBinPath, "publish.sh")
      ),
      new FileCopyRequest(
        dependencies.CombinePath(templatesBinPath, "validate.sh"),
        dependencies.CombinePath(ciBinPath, "validate.sh")
      )
    };
  }

  private static IReadOnlyDictionary<string, string> GetTemplateValues(TemplateInitRequest request)
  {
    return new Dictionary<string, string>();
  }

  public static async Task<Result<TemplateInitResult>> TryHandleRequest(CommandDependencies dependencies,
    TemplateInitRequest request)
  {
    dependencies.StandardOutWriteLine("Initializing project...\n");
    return await (await Validation.ValidateRequestExecution(dependencies, request)
        .TapFailure(exception =>
          dependencies.StandardOutWriteLine(
            $"Request cannot be completed.\nError: {exception.GetType()}\nMessage: {exception.Message}"))
        .BindAsync(context => TryWriteMetadataFile(dependencies, context)))
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
          dependencies.StandardOutWriteLine("Initialization complete.");
          dependencies.StandardOutWriteAsLine(new[]
          {
            ((ConsoleColor?)null, "Project metadata updated:"), (ConsoleColor.Yellow, validatedRequest.MetadataFile)
          });
          dependencies.StandardOutWriteLine(string.Empty);
          dependencies.StandardOutWriteLine("Files:");
          foreach (var result in results)
          {
            dependencies.StandardOutWriteAsLine(new[]
            {
              (result.Written ? (ConsoleColor?)ConsoleColor.Green : ConsoleColor.Gray,
                result.Written ? "  Copied " : "  Skipped"),
              (result.Written ? ConsoleColor.DarkYellow : ConsoleColor.DarkGray, $" {result.Request.DestinationPath}")
            });
          }
        })
        .Map(_ => new TemplateInitResult(validatedRequest.ProjectRoot, validatedRequest.OverwriteFiles))
        .TapSuccess(result => DisplayNextSteps(dependencies, result))
      );
  }

  private static async Task<Result<TemplateInitContext>> TryWriteMetadataFile(CommandDependencies dependencies,
    TemplateInitContext context)
  {
    return context.MetadataFile.ToLowerInvariant().Contains("package.json")
      // Don't update package.json files.
      ? new Result<TemplateInitContext>(context)
      : (await Json.TrySerialize(context.ProjectMetadata)
        .BindAsync(json => dependencies.TryWriteFileStringAsync((FileName: context.MetadataFile, Content: json)))
      )
      .Map(_ => context);
  }

  private static void DisplayNextSteps(CommandDependencies dependencies, TemplateInitResult initResult)
  {
    var ciPath = dependencies.CombinePath(initResult.ProjectRoot, Conventions.CiDirectoryName);
    var ciBinPath = dependencies.CombinePath(ciPath, Conventions.CiBinDirectoryName);
    var ciLibExecPath = dependencies.CombinePath(ciPath, Conventions.CiLibExecDirectoryName);
    var ciLibExecWorkflowsPath = dependencies.CombinePath(ciLibExecPath, Conventions.CiLibExecWorkflowsDirectoryName);

    void WriteColorizedNode(ConsoleColor titleColor, string title, string description)
    {
      var validateSteps = new[]
      {
        ((ConsoleColor?)null, " * "), (titleColor, title), ((ConsoleColor?)null, description)
      };
      dependencies.StandardOutWriteAsLine(validateSteps);
    }

    dependencies.StandardOutWriteAsLine(new[]
    {
      ((ConsoleColor?)null, @"
Continuous integration scripts initialized successfully.

The following CI entrypoints were initialized in "),
      ((ConsoleColor?)ConsoleColor.Yellow, ciBinPath), ((ConsoleColor?)null, ":")
    });

    WriteColorizedNode(ConsoleColor.Blue, "validate.sh", @" - Validate the project code.
                 By default, this runs the ci-validate and ci-compose workflows
                 using 'cicee lib exec'.
   Expected use: Execute during pull request review to provide static code
                 analysis and run tests.
");
    WriteColorizedNode(ConsoleColor.Blue, "publish.sh", @"  - Builds and publishes the project distributable artifacts.
                 E.g., push a Docker image, publish an NPM package
                 By default, this runs the ci-compose and ci-publish workflows 
                 using 'cicee lib exec'.
   Expected use: Execute after a project repository merge creates a new project
                 release. E.g., after a merge to 'main' or 'trunk'
");

    dependencies.StandardOutWriteAsLine(new[]
    {
      ((ConsoleColor?)null, "The following CI workflows were initialized in "),
      ((ConsoleColor?)ConsoleColor.Yellow, ciLibExecWorkflowsPath), ((ConsoleColor?)null, ":")
    });

    WriteColorizedNode(ConsoleColor.Magenta, "ci-validate", @"(.sh) - Validates the project code.
                      E.g., compile source, run tests, lint");
    WriteColorizedNode(ConsoleColor.Magenta, "ci-compose", @"(.sh)  - Publishes the project distributable artifacts.
                      Assumes ci-compose previously generated artifacts.
                      E.g., push a Docker image, publish an NPM package");

    var nextSteps = $@"
Next steps:
  * Add execute permission to all initialized scripts.
    If using macOS or Linux:
      Run the following from your shell (from {initResult.ProjectRoot}):
      chmod +x ci/bin/*.sh ci/libexec/workflows/*.sh
    If using Windows:
      Run the following from Git Bash (from {initResult.ProjectRoot}):
      git add ci/bin/*.sh ci/libexec/workflows/*.sh && git update-index --chmod=+x ci/bin/*.sh ci/libexec/workflows/*.sh
  * Update the workflows in {ciLibExecWorkflowsPath}.
    Setup the continuous integration processes the project needs.
";

    dependencies.StandardOutWriteLine(nextSteps);
  }
}
