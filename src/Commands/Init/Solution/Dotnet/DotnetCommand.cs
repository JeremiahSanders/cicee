using System.CommandLine;
using Cicee.Commands.Init.Solution.Options;
using Cicee.Dependencies;

namespace Cicee.Commands.Init.Solution.Dotnet;

public static class DotnetCommand
{
  public static Option<bool> UseDomainOption()
  {
    return new Option<bool>(new[] {"--use-domain"});
  }

  public static Option<bool> UseUnitTestsOption()
  {
    return new Option<bool>(new[] {"--use-unit-tests"});
  }

  public static Option<bool> UseIntegrationTestsOption()
  {
    return new Option<bool>(new[] {"--use-integration-tests"});
  }

  public static Option<string?> ApplicationTemplateOption()
  {
    return new Option<string?>(new[] {"--application-template"});
  }

  public static Command Create(CommandDependencies dependencies)
  {
    var dotnetLanguageOption = DotnetLanguageOption.Create();
    var solutionNameOption = DotnetSolutionNameOption.Create();
    solutionNameOption.IsRequired = true;
    var dotnetNamespacePrefixOption = DotnetNamespacePrefixOption.Create();
    var projectRootOption = ProjectRootOption.Create(dependencies);
    var applicationTemplateOption = ApplicationTemplateOption();

    var useDomainOption = UseDomainOption();
    var useUnitTestsOption = UseUnitTestsOption();
    var useIntegrationTestsOption = UseIntegrationTestsOption();

    var command = new Command("dotnet")
    {
      dotnetLanguageOption,
      solutionNameOption,
      dotnetNamespacePrefixOption,
      projectRootOption,
      applicationTemplateOption,
      useDomainOption,
      useUnitTestsOption,
      useIntegrationTestsOption
    };

    var handlerFunc = DotnetSolutionInitializationHandling.CreateHandler(dependencies);
    command.SetHandler(handlerFunc,
      dotnetLanguageOption, solutionNameOption, dotnetNamespacePrefixOption,
      projectRootOption,
      applicationTemplateOption,
      useDomainOption, useUnitTestsOption, useIntegrationTestsOption);

    return command;
  }
}
