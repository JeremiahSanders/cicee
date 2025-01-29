using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

using Cicee.CiEnv;
using Cicee.Dependencies;

using LanguageExt.Common;

namespace Cicee.Commands.Exec.Handling;

[SuppressMessage(category: "ReSharper", checkId: "UnusedParameter.Local")]
public static class IoContext
{
  private static string CreateCiDockerfilePath(CommandDependencies dependencies, ExecRequest request)
  {
    return dependencies.CombinePath(
      request.ProjectRoot,
      dependencies.CombinePath(HandlingConstants.CiDirectoryName, arg2: "Dockerfile")
    );
  }

  public static string CreateCiDockerfileImageTag(string projectMetadataName)
  {
    // TODO: Add a hash... preferably the Dockerfile
    string modified = projectMetadataName
      .Replace(oldValue: " ", string.Empty)
      .Replace(oldValue: "\t", string.Empty)
      .Replace(oldValue: "\r", string.Empty)
      .Replace(oldValue: "\n", string.Empty)
      .ToLowerInvariant()
      .ToKebabCase();
    if (string.IsNullOrWhiteSpace(modified))
    {
      modified = DateTime.Now.ToString(format: "yyyyMMdd-HHmmss");
    }

    return $"ci-env-{modified}";
  }

  public static ExecRequestContext DisplayExecContext(
    CommandDependencies dependencies,
    ExecRequestContext execRequestContext
  )
  {
    DisplayProjectEnvironmentValues();
    DisplayExecEnvironmentValues();

    return execRequestContext;

    void WriteEnvironmentVariables(IReadOnlyDictionary<string, string> environmentDisplay)
    {
      int width = environmentDisplay.Keys.Max(value => value.Length) + 1;
      foreach ((string key, string value) in environmentDisplay.OrderBy(kvp => kvp.Key))
      {
        dependencies.StandardOutWriteLine($"  {key.PadRight(width, paddingChar: ' ')}: {value}");
      }
    }

    void DisplayProjectEnvironmentValues()
    {
      ProjectEnvironmentHelpers.DisplayProjectEnvironmentValues(
        dependencies.StandardOutWriteLine,
        dependencies.StandardOutWrite,
        ProjectEnvironmentHelpers.GetEnvironmentDisplay(
          dependencies.GetEnvironmentVariables,
          execRequestContext.ProjectMetadata
        )
      );
    }

    void DisplayExecEnvironmentValues()
    {
      IReadOnlyDictionary<string, string> environmentDisplay =
        IoEnvironment.GetExecEnvironment(dependencies, execRequestContext, forcePathsToLinux: false);
      dependencies.StandardOutWriteLine(obj: "CICEE Execution Environment:");
      WriteEnvironmentVariables(environmentDisplay);
    }
  }

  public static Result<ExecRequestContext> TryCreateRequestContext(
    CommandDependencies dependencies,
    ExecRequest request)
  {
    return dependencies
      .EnsureDirectoryExists(request.ProjectRoot)
      .Bind(
        validatedProjectRoot => ProjectMetadataLoader
          .TryFindProjectMetadata(
            dependencies.EnsureDirectoryExists,
            dependencies.EnsureFileExists,
            dependencies.TryLoadFileString,
            dependencies.CombinePath,
            validatedProjectRoot
          )
          .Match(
            value => new Result<ProjectMetadata>(value.ProjectMetadata),
            loadFailure =>
              // We failed to load metadata, but we know that the project root exists.
              ProjectMetadataLoader.InferProjectMetadata(dependencies, validatedProjectRoot)
          )
      )
      .Bind(
        projectMetadata => Require
          .AsResult.NotNullOrWhitespace(request.Image)
          .BindFailure(_ => dependencies.EnsureFileExists(CreateCiDockerfilePath(dependencies, request)))
          .MapFailure(
            exception => new BadRequestException(
              $"Image argument was not provided and '{CreateCiDockerfilePath(dependencies, request)}; does not exist.",
              exception
            )
          )
          .Map(_ => projectMetadata)
      )
      .Map(
        projectMetadata =>
        {
          string? ciDirectory = dependencies
            .EnsureDirectoryExists(dependencies.CombinePath(request.ProjectRoot, HandlingConstants.CiDirectoryName))
            .Match(string? (dir) => dir, _ => null);
          string? dockerfile = ciDirectory == null
            ? null
            : dependencies
              .EnsureFileExists(dependencies.CombinePath(ciDirectory, arg2: "Dockerfile"))
              .Match(string? (file) => file, _ => null);
          string[] dockerComposeFiles = GetDockerComposeFiles();

          string? libRoot = dependencies
            .EnsureDirectoryExists(
              dependencies.CombinePath(
                request.ProjectRoot,
                dependencies.CombinePath(HandlingConstants.CiDirectoryName, Conventions.CiLibDirectoryName)
              )
            )
            .Match(string? (dir) => dir, _ => null);

          // TODO: Reconsider keeping this image tag. Could be useful for caching. Currently automatic image build in direct harness is disabled. 
          string ciDockerfileImageTag = CreateCiDockerfileImageTag(projectMetadata.Name);

          string? image = !string.IsNullOrWhiteSpace(request.Image)
            ? request.Image
            : null;

          return new ExecRequestContext(
            request.ProjectRoot,
            projectMetadata,
            request.Command,
            request.Entrypoint,
            dockerfile,
            image,
            request.Harness,
            request.Verbosity,
            ciDirectory,
            dockerComposeFiles,
            libRoot,
            ciDockerfileImageTag
          );
        }
      );

    IEnumerable<string> EnumerateIfExists(string pathToCheck, string? fallback = null)
    {
      return dependencies
        .EnsureFileExists(pathToCheck)
        .BindFailure(
          exception => fallback != null ? dependencies.EnsureFileExists(fallback) : new Result<string>(exception)
        )
        .Map(
          value => (IEnumerable<string>)new[]
          {
            value
          }
        )
        .IfFail(Enumerable.Empty<string>());
    }

    IEnumerable<string> EnumerateChildFileIfExists(
      string directory,
      string fileName,
      string? fallbackDirectory = null,
      string? fallbackFileName = null)
    {
      return EnumerateIfExists(
        dependencies.CombinePath(directory, fileName),
        fallbackFileName == null ? null : dependencies.CombinePath(fallbackDirectory ?? directory, fallbackFileName)
      );
    }

    string[] GetDockerComposeFiles()
    {
      /*
declare -r DOCKERCOMPOSE_DEPENDENCIES_CI="${PROJECT_ROOT}/ci/docker-compose.dependencies.yml"
declare -r DOCKERCOMPOSE_DEPENDENCIES_ROOT="${PROJECT_ROOT}/docker-compose.ci.dependencies.yml"
declare -r DOCKERCOMPOSE_CICEE="${LIB_ROOT}/docker-compose.yml"
declare -r DOCKERCOMPOSE_PROJECT_CI="${PROJECT_ROOT}/ci/docker-compose.project.yml"
declare -r DOCKERCOMPOSE_PROJECT_ROOT="${PROJECT_ROOT}/docker-compose.ci.project.yml"
       */
      string ciceeLibRoot = dependencies.GetLibraryRootPath();
      string projectRoot = request.ProjectRoot;
      string ciRoot = dependencies.CombinePath(projectRoot, HandlingConstants.CiDirectoryName);

      IEnumerable<string> composeFiles = ArraySegment<string>.Empty;
// Use project docker-compose as the primary file (by loading it first). Affects docker container name generation.
      composeFiles = composeFiles.Concat(
        EnumerateChildFileIfExists(
          ciRoot,
          fileName: "docker-compose.project.yml",
          projectRoot,
          fallbackFileName: "docker-compose.ci.project.yml"
        )
      );
// Add dependencies
      composeFiles = composeFiles.Concat(
        EnumerateChildFileIfExists(
          ciRoot,
          fileName: "docker-compose.dependencies.yml",
          projectRoot,
          fallbackFileName: "docker-compose.ci.dependencies.yml"
        )
      );
// Add CICEE
      composeFiles = composeFiles.Concat(
        new[]
        {
          dependencies.CombinePath(ciceeLibRoot, arg2: "docker-compose.yml")
        }
      );
// - Import the ci-exec service image source (Dockerfile or image)
      bool useImage = !string.IsNullOrWhiteSpace(request.Image); // || request.Harness == ExecInvocationHarness.Direct;
      composeFiles = composeFiles.Concat(
        useImage
          ? new[]
          {
            dependencies.CombinePath(ciceeLibRoot, arg2: "docker-compose.image.yml")
          }
          : new[]
          {
            dependencies.CombinePath(ciceeLibRoot, arg2: "docker-compose.dockerfile.yml")
          }
      );
// Re-add project, to load project settings last (to override all other dependencies, e.g., CICEE defaults).
      composeFiles = composeFiles.Concat(
        EnumerateChildFileIfExists(
          ciRoot,
          fileName: "docker-compose.project.yml",
          projectRoot,
          fallbackFileName: "docker-compose.ci.project.yml"
        )
      );


      return composeFiles.ToArray();
    }
  }

  public static Result<ExecRequestContext> ValidateContext(
    CommandDependencies dependencies,
    ExecRequestContext execRequestContext
  )
  {
    return RequireStartupCommand(execRequestContext)
      .Bind(RequireProjectRoot);

    static Result<ExecRequestContext> RequireStartupCommand(ExecRequestContext context)
    {
      // Require either a command
      return Require
        .AsResult.NotNullOrWhitespace(context.Command)
        .BindFailure(
          missingCommandException =>
            // ... or an entrypoint
            Require.AsResult.NotNullOrWhitespace(context.Entrypoint)
        )
        .MapFailure(
          exception => new BadRequestException(
            message: "At least one of command or entrypoint must be provided.",
            exception
          )
        )
        .Map(_ => context);
    }

    Result<ExecRequestContext> RequireProjectRoot(ExecRequestContext contextWithStartupCommand)
    {
      return dependencies
        .EnsureDirectoryExists(contextWithStartupCommand.ProjectRoot)
        .MapFailure(
          exception => exception is DirectoryNotFoundException
            ? new BadRequestException($"Project root '{contextWithStartupCommand.ProjectRoot}' cannot be found.")
            : exception
        )
        .Map(projectRoot => contextWithStartupCommand);
    }
  }
}
