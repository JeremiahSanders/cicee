using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Cicee.CiEnv;
using Cicee.Dependencies;
using Cicee.Extensions;

using LanguageExt.Common;

namespace Cicee.Commands.Template.Init;

public static class TemplateInitHandling
{
  private static IReadOnlyCollection<FileCopyRequest> GetFiles(
    ICommandDependencies dependencies,
    TemplateInitContext request)
  {
    string templatesBinPath = dependencies.CombinePath(
      dependencies.GetInitTemplatesDirectoryPath(),
      Conventions.CiBinDirectoryName
    );
    string templatesLibExecPath = dependencies.CombinePath(
      dependencies.GetInitTemplatesDirectoryPath(),
      Conventions.CiLibExecDirectoryName
    );
    string templatesLibExecWorkflowsPath = dependencies.CombinePath(
      templatesLibExecPath,
      Conventions.CiLibExecWorkflowsDirectoryName
    );
    string ciPath = dependencies.CombinePath(request.ProjectRoot, Conventions.CiDirectoryName);
    string ciBinPath = dependencies.CombinePath(ciPath, Conventions.CiBinDirectoryName);
    string ciLibExecPath = dependencies.CombinePath(ciPath, Conventions.CiLibExecDirectoryName);
    string ciLibExecWorkflowsPath = dependencies.CombinePath(
      ciLibExecPath,
      Conventions.CiLibExecWorkflowsDirectoryName
    );

    return new[]
    {
      new FileCopyRequest(
        dependencies.CombinePath(templatesLibExecWorkflowsPath, suffix: "ci-compose.sh"),
        dependencies.CombinePath(ciLibExecWorkflowsPath, suffix: "ci-compose.sh")
      ),
      new FileCopyRequest(
        dependencies.CombinePath(templatesLibExecWorkflowsPath, suffix: "ci-publish.sh"),
        dependencies.CombinePath(ciLibExecWorkflowsPath, suffix: "ci-publish.sh")
      ),
      new FileCopyRequest(
        dependencies.CombinePath(templatesLibExecWorkflowsPath, suffix: "ci-validate.sh"),
        dependencies.CombinePath(ciLibExecWorkflowsPath, suffix: "ci-validate.sh")
      ),
      new FileCopyRequest(
        dependencies.CombinePath(templatesBinPath, suffix: "publish.sh"),
        dependencies.CombinePath(ciBinPath, suffix: "publish.sh")
      ),
      new FileCopyRequest(
        dependencies.CombinePath(templatesBinPath, suffix: "validate.sh"),
        dependencies.CombinePath(ciBinPath, suffix: "validate.sh")
      )
    };
  }

  private static IReadOnlyDictionary<string, string> GetTemplateValues(TemplateInitRequest request)
  {
    return new Dictionary<string, string>();
  }


  public static async Task<Result<TemplateInitResult>> TryHandleRequest(
    ICommandDependencies dependencies,
    TemplateInitRequest request)
  {
    dependencies.StandardOutWriteLine(text: "Initializing project...\n");

    return await
      (await (await Validation
        .ValidateRequestExecution(dependencies, request)
        .TapFailure(
          exception => dependencies.StandardOutWriteLine(
            $"Request cannot be completed.\nError: {exception.GetType()}\nMessage: {exception.Message}"
          )
        )
        .BindAsync(
          async context => (await dependencies.TryAddCiceeLocalToolAsync(context.ProjectRoot))
            .Map(_ => context)
            .TapFailure(
              exception => dependencies.StandardErrorWriteLine(
                $"Failed to install CICEE as a .NET local tool.\nError: {exception.GetType()}\nMessage: {exception.Message}"
              )
            )
        )).BindAsync(
        async context =>
          (await dependencies.TryWriteMetadataFile(context.MetadataFile, context.ProjectMetadata)).Map(_ => context)
      )).BindAsync(
        async validatedRequest => (await FileCopyHelpers.TryWriteFiles(
            dependencies,
            GetFiles(dependencies, validatedRequest),
            GetTemplateValues(request),
            validatedRequest.OverwriteFiles
          ))
          .TapFailure(
            exception => dependencies.StandardOutWriteLine(
              $"Failed to write files.\nError: {exception.GetType()}\nMessage: {exception.Message}"
            )
          )
          .TapSuccess(
            results =>
            {
              dependencies.StandardOutWriteLine(text: "Initialization complete.");
              dependencies.StandardOutWriteAsLine(
                new[]
                {
                  ((ConsoleColor?)null, "Project metadata updated:"),
                  (ConsoleColor.Yellow, validatedRequest.MetadataFile)
                }
              );
              dependencies.StandardOutWriteLine(string.Empty);
              dependencies.StandardOutWriteLine(text: "Files:");
              foreach (FileCopyResult result in results)
              {
                dependencies.StandardOutWriteAsLine(
                  new[]
                  {
                    (result.Written ? (ConsoleColor?)ConsoleColor.Green : ConsoleColor.Gray,
                      result.Written ? "  Copied " : "  Skipped"),
                    (result.Written ? ConsoleColor.DarkYellow : ConsoleColor.DarkGray,
                      $" {result.Request.DestinationPath}")
                  }
                );
              }
            }
          )
          .Map(_ => new TemplateInitResult(validatedRequest.ProjectRoot, validatedRequest.OverwriteFiles))
          .TapSuccess(result => DisplayNextSteps(dependencies, result))
      );
  }

  private static void DisplayNextSteps(ICommandDependencies dependencies, TemplateInitResult initResult)
  {
    string ciPath = dependencies.CombinePath(initResult.ProjectRoot, Conventions.CiDirectoryName);
    string ciBinPath = dependencies.CombinePath(ciPath, Conventions.CiBinDirectoryName);
    string ciLibExecPath = dependencies.CombinePath(ciPath, Conventions.CiLibExecDirectoryName);
    string ciLibExecWorkflowsPath = dependencies.CombinePath(
      ciLibExecPath,
      Conventions.CiLibExecWorkflowsDirectoryName
    );

    dependencies.StandardOutWriteAsLine(
      new[]
      {
        (null, @"
Continuous integration scripts initialized successfully.

The following CI entrypoints were initialized in "),
        (ConsoleColor.Yellow, ciBinPath),
        ((ConsoleColor?)null, ":")
      }
    );

    WriteColorizedNode(
      ConsoleColor.Blue,
      title: "validate.sh",
      description: @" - Validate the project code.
                 By default, this runs the ci-validate and ci-compose workflows
                 using 'cicee lib exec'.
   Expected use: Execute during pull request review to provide static code
                 analysis and run tests.
"
    );
    WriteColorizedNode(
      ConsoleColor.Blue,
      title: "publish.sh",
      description: @"  - Builds and publishes the project distributable artifacts.
                 E.g., push a Docker image, publish an NPM package
                 By default, this runs the ci-compose and ci-publish workflows 
                 using 'cicee lib exec'.
   Expected use: Execute after a project repository merge creates a new project
                 release. E.g., after a merge to 'main' or 'trunk'
"
    );

    dependencies.StandardOutWriteAsLine(
      new[]
      {
        (null, "The following CI workflows were initialized in "),
        (ConsoleColor.Yellow, ciLibExecWorkflowsPath),
        ((ConsoleColor?)null, ":")
      }
    );

    WriteColorizedNode(
      ConsoleColor.Magenta,
      title: "ci-validate",
      description: @"(.sh) - Validates the project code.
                      E.g., compile source, run tests, lint"
    );
    WriteColorizedNode(
      ConsoleColor.Magenta,
      title: "ci-compose",
      description: @"(.sh)  - Publishes the project distributable artifacts.
                      Assumes ci-compose previously generated artifacts.
                      E.g., push a Docker image, publish an NPM package"
    );

    string nextSteps = $@"
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

    return;

    void WriteColorizedNode(ConsoleColor titleColor, string title, string description)
    {
      (ConsoleColor?, string)[] validateSteps =
      {
        (null, " * "),
        (titleColor, title),
        (null, description)
      };
      dependencies.StandardOutWriteAsLine(validateSteps);
    }
  }
}
