using System;
using System.Threading.Tasks;
using Cicee.Commands.Init.Solution.Dotnet.PlannedInitializers;
using Cicee.Commands.Init.Solution.Options;
using Cicee.Dependencies;
using LanguageExt;

namespace Cicee.Commands.Init.Solution.Dotnet;

public static class DotnetSolutionInitializationHandling
{
  public static Task<DotnetSolutionInitializationExecutionResult> HandleRequest(CommandDependencies dependencies,
    DotnetSolutionInitializationRequest request)
  {
    var language = DotnetLanguageOption.GetLanguage(request.Language);
    var baseCommands = new[]
    {
      DotnetCliCommand.NewGlobalJson(), DotnetCliCommand.NewSolution(request.SolutionName, request.ProjectRoot),
      DotnetCliCommand.NewProject(request.Application.Directory, request.Application.DotnetTemplate,
        request.Application.AssemblyName, language),
      DotnetCliCommand.AddProjectToSolution(request.ProjectRoot, request.Application.Directory)
    };
    var unitTestCommands = request.UnitTests != null
      ? new[]
      {
        DotnetCliCommand.NewProject(request.UnitTests, language),
        DotnetCliCommand.ReferenceProject(request.UnitTests.Directory, request.Application.Directory),
        DotnetCliCommand.AddProjectToSolution(request.ProjectRoot, request.UnitTests.Directory)
      }
      : Array.Empty<DotnetCliCommand>();
    var integrationTestCommands = request.IntegrationTests != null
      ? new[]
      {
        DotnetCliCommand.NewProject(request.IntegrationTests, language),
        DotnetCliCommand.ReferenceProject(request.IntegrationTests.Directory, request.Application.Directory),
        DotnetCliCommand.AddProjectToSolution(request.ProjectRoot, request.IntegrationTests.Directory)
      }
      : Array.Empty<DotnetCliCommand>();

    throw new NotImplementedException();
  }

  public static Func<DotnetLanguageOption.DotnetLanguage?, string?, string?, string, string?, bool, bool, bool,
      Task<int>>
    CreateHandler(CommandDependencies dependencies)
  {
    async Task<int> HandleDotnetCommand(
      DotnetLanguageOption.DotnetLanguage? dotnetLanguage,
      string? solutionName,
      string? dotnetNamespacePrefix,
      string projectRoot,
      string? applicationTemplate,
      bool useDomain,
      bool useUnitTests,
      bool useIntegrationTests
    )
    {
      DotnetSolutionInitializationRequest ParseParameters()
      {
        if (string.IsNullOrWhiteSpace(solutionName))
        {
          throw new BadRequestException("Solution name is required.");
        }

        if (string.IsNullOrWhiteSpace(applicationTemplate))
        {
          throw new BadRequestException("Application template is required.");
        }

        const string sourceDirectoryName = "src";
        const string testsDirectoryName = "tests";

        var sourceDir = dependencies.CombinePath(projectRoot, sourceDirectoryName);
        var testsDir = dependencies.CombinePath(projectRoot, testsDirectoryName);

        var defaultedPrefix = (dotnetNamespacePrefix ?? "").Trim();
        var namespacePrefix = defaultedPrefix.EndsWith(".") ? defaultedPrefix[..^1] : defaultedPrefix;
        var namespaceWithDot = string.IsNullOrEmpty(namespacePrefix) ? string.Empty : $"{namespacePrefix}.";

        var application = new DotnetProjectParameters
        {
          Directory = useDomain ? dependencies.CombinePath(sourceDir, solutionName) : sourceDir,
          AssemblyName = $"{namespaceWithDot}{solutionName}",
          DotnetTemplate = applicationTemplate
        };

        var domain = useDomain
          ? new DotnetProjectParameters
          {
            Directory = dependencies.CombinePath(sourceDir, "domain"),
            AssemblyName = $"{namespaceWithDot}{solutionName}.Domain"
          }
          : null;

        var integrationTests = useIntegrationTests
          ? new DotnetProjectParameters
          {
            Directory = dependencies.CombinePath(testsDir, "integration"),
            AssemblyName = $"{namespaceWithDot}{solutionName}.Tests.Integration",
            DotnetTemplate = "xunit"
          }
          : null;
        var unitTests = useUnitTests
          ? new DotnetProjectParameters
          {
            Directory = dependencies.CombinePath(testsDir, "unit"),
            AssemblyName = $"{namespaceWithDot}{solutionName}.Tests.Unit",
            DotnetTemplate = "xunit"
          }
          : null;
        var request = new DotnetSolutionInitializationRequest
        {
          SolutionName = solutionName,
          Application = application,
          Domain = domain,
          Language = dotnetLanguage ?? DotnetLanguageOption.DotnetLanguage.CSharp,
          IntegrationTests = integrationTests,
          UnitTests = unitTests,
          ProjectRoot = projectRoot,
          DotnetNamespacePrefix = namespacePrefix
        };
        return request;
      }

      return (await
          Prelude.Try(ParseParameters).Try()
            .BindAsync(request =>
              Prelude.TryAsync(HandleRequest(dependencies, request)).Try())
        )
        .ToExitCode();
    }

    return HandleDotnetCommand;
  }
}
