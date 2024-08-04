using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Cicee.CiEnv;
using Cicee.Commands.Lib;
using Cicee.Dependencies;

using LanguageExt.Common;

namespace Cicee.Commands.Template.Lib;

public static class TemplateLibHandling
{
  private static DirectoryCopyRequest CreateLibraryDirectoryCopyRequest(CommandDependencies dependencies,
    TemplateLibContext request)
  {
    string ciPath = dependencies.CombinePath(request.ProjectRoot, Conventions.CiDirectoryName);
    string ciLibPath = dependencies.CombinePath(ciPath, Conventions.CiLibDirectoryName);
    return new DirectoryCopyRequest(dependencies.GetLibraryRootPath(), ciLibPath, request.OverwriteFiles);
  }

  private static IReadOnlyCollection<FileCopyRequest> CreateCiceeExecCopyRequests(CommandDependencies dependencies,
    TemplateLibContext request)
  {
    return Array.Empty<FileCopyRequest>();
  }

  private static async Task<Result<(string CiceeExecEntrypoint, IReadOnlyCollection<FileCopyResult> FileCopyResults)>>
    CopyCiceeExec(CommandDependencies dependencies, TemplateLibRequest request,
      IReadOnlyCollection<FileCopyRequest> fileCopyRequests)
  {
    return (await FileCopyHelpers.TryWriteFiles(
      dependencies,
      fileCopyRequests,
      GetTemplateValues(request),
      request.OverwriteFiles
    )).TapFailure(
      exception => dependencies.StandardOutWriteLine(
        $"Failed to write CICEE execution scripts.\nError: {exception.GetType()}\nMessage: {exception.Message}"
      )
    ).TapSuccess(
      results =>
      {
        dependencies.StandardOutWriteLine(obj: "CICEE execution script initialization complete.");
        dependencies.StandardOutWriteLine(obj: "Files:");
        foreach (FileCopyResult result in results)
        {
          dependencies.StandardOutWriteLine(
            $"  {(result.Written ? "Copied " : "Skipped")} {result.Request.DestinationPath}"
          );
        }
      }
    ).Map(
      results => (
        dependencies.CombinePath(
          dependencies.CombinePath(
            dependencies.CombinePath(request.ProjectRoot, Conventions.CiDirectoryName),
            Conventions.CiLibDirectoryName
          ),
          arg2: "cicee-exec.sh"
        ), results)
    );
  }

  private static async Task<Result<(string CiLibEntrypoint, DirectoryCopyResult CiLibDirectoryCopyResult)>>
    CopyCiLibrary(CommandDependencies dependencies, DirectoryCopyRequest request)
  {
    dependencies.StandardOutWriteLine(obj: "Copying CICEE CI action library...");
    dependencies.StandardOutWriteLine($"  Source     : {request.SourceDirectoryPath}");
    dependencies.StandardOutWriteLine($"  Destination: {request.DestinationDirectoryPath}");

    return (await dependencies.TryCopyDirectoryAsync(request)).TapFailure(
      exception => dependencies.StandardErrorWriteLine(
        $"Failed to copy CICEE CI action library.\nError: {exception.GetType()}\nMessage: {exception.Message}"
      )
    ).TapSuccess(
      results =>
      {
        dependencies.StandardOutWriteLine(obj: "CICEE CI action library initialized.");

        dependencies.StandardOutWriteLine(obj: "Directories created:");
        if (results.CreatedDirectories.Count == 0)
        {
          dependencies.StandardOutWriteLine(obj: "  None");
          dependencies.StandardOutWriteLine(obj: "");
        }
        else
        {
          foreach ((string SourceDirectory, string DestinationDirectory) createdTuple in results.CreatedDirectories)
          {
            dependencies.StandardOutWriteLine($"  {createdTuple.DestinationDirectory}");
          }

          dependencies.StandardOutWriteLine(obj: "");
        }

        dependencies.StandardOutWriteLine(obj: "Files copied:");
        if (results.CopiedFiles.Count == 0)
        {
          dependencies.StandardOutWriteLine(obj: "  None");
          dependencies.StandardOutWriteLine(obj: "");
        }
        else
        {
          foreach ((string SourceFile, string DestinationFile) copiedTuple in results.CopiedFiles)
          {
            dependencies.StandardOutWriteLine($"  {copiedTuple.DestinationFile}");
          }

          dependencies.StandardOutWriteLine(obj: "");
        }
      }
    ).Map(
      directoryCopyResult => (dependencies.CombinePath(directoryCopyResult.DestinationDirectoryPath, arg2: "ci.sh"),
        directoryCopyResult)
    );
  }

  private static IReadOnlyDictionary<string, string> GetTemplateValues(TemplateLibRequest request)
  {
    return new Dictionary<string, string>();
  }

  public static async Task<Result<TemplateLibResult>> TryHandleRequest(CommandDependencies dependencies,
    TemplateLibRequest request)
  {
    dependencies.StandardOutWriteLine(obj: "Initializing project with CICEE execution library...\n");
    Result<TemplateLibContext> validationResult = await Validation.ValidateRequestAsync(dependencies, request);
    return (await validationResult
      .TapFailure(
        exception => dependencies.StandardOutWriteLine(
          $"Request cannot be completed.\nError: {exception.GetType()}\nMessage: {exception.Message}"
        )
      ).BindAsync(
        async validatedRequest => await (await CopyCiLibrary(
          dependencies,
          CreateLibraryDirectoryCopyRequest(dependencies, validatedRequest)
        )).BindAsync(
          async ciLibraryCopyResult =>
          {
            return (await CopyCiceeExec(
              dependencies,
              request,
              CreateCiceeExecCopyRequests(dependencies, validatedRequest)
            )).Map(
              ciceeExecCopyResults => new TemplateLibResult(
                validatedRequest.ProjectRoot,
                validatedRequest.ShellTemplate,
                validatedRequest.LibPath,
                validatedRequest.OverwriteFiles,
                ciLibraryCopyResult.CiLibDirectoryCopyResult,
                ciceeExecCopyResults.FileCopyResults,
                ciLibraryCopyResult.CiLibEntrypoint,
                ciceeExecCopyResults.CiceeExecEntrypoint
              )
            );
          }
        )
      )).TapSuccess(result => DisplayNextSteps(dependencies, result));
  }

  private static void DisplayNextSteps(CommandDependencies dependencies, TemplateLibResult libResult)
  {
    string shellSteps = libResult.ShellTemplate switch
    {
      LibraryShellTemplate.Bash => $@"
Assuming a present working directory of the project root:
  {libResult.ProjectRoot}

Example execution of validation workflow:

$ CI_ENTRYPOINT=""ci/bin/validate.sh"" .{libResult.CiceeExecEntrypointPath.Substring(libResult.ProjectRoot.Length)}

Example execution of publish workflow from a specific shell:

$ CI_ENTRYPOINT=""/bin/bash"" CI_COMMAND=""ci/bin/publish.sh"" .{libResult.CiceeExecEntrypointPath.Substring(libResult.ProjectRoot.Length)}
",
      _ => string.Empty
    };
    string nextSteps = $@"
CICEE execution library initialized successfully.

Two notable components were installed: a CICEE execution script and a CI
library script. Using these components, you may now execute a CI workflow in a
'cicee exec'-like manner, without having the CICEE executable installed.
For example, on a continuous integration server.

To run a CI workflow script:

{shellSteps}
";

    dependencies.StandardOutWriteLine(nextSteps);
  }
}
