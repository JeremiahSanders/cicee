using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cicee.Commands.Init.Solution.Dotnet.PlannedInitializers;

public record DotnetCliCommand : ICreateProcessStartInfo
{
  public IReadOnlyList<string> Arguments { get; init; } = ArraySegment<string>.Empty;

  public ProcessStartInfo ToProcessStartInfo()
  {
    var startInfo = new ProcessStartInfo("dotnet");

    foreach (var argument in Arguments)
    {
      startInfo.ArgumentList.Add(argument);
    }

    return startInfo;
  }

  public static DotnetCliCommand NewProject(string outputDirectory, string projectTemplate, string projectName,
    string projectLanguage,
    string? projectFramework = null)
  {
    var arguments = new[] {"new"};

    var template = new[] {projectTemplate};
    var output = new[] {"--output", outputDirectory};
    var name = new[] {"--name", projectName};
    var language = new[] {"--language", projectLanguage};
    var framework = projectFramework != null ? new[] {"--framework", projectFramework} : Array.Empty<string>();

    return new DotnetCliCommand
    {
      Arguments = arguments.Append(template).Append(output).Append(name).Append(language).Append(framework).ToList()
    };
  }

  public static DotnetCliCommand NewProject(DotnetProjectParameters dotnetProjectParameters, string language)
  {
    return NewProject(dotnetProjectParameters.Directory, dotnetProjectParameters.DotnetTemplate,
      dotnetProjectParameters.AssemblyName, language);
  }

  public static DotnetCliCommand NewGlobalJson(string? sdkVersion = null, string? rollForward = null)
  {
    var arguments = new[] {"new", "globaljson"};

    var sdk = sdkVersion != null ? new[] {"--sdk-version", sdkVersion} : Array.Empty<string>();
    var roll = rollForward != null ? new[] {"--roll-forward", rollForward} : Array.Empty<string>();

    return new DotnetCliCommand {Arguments = arguments.Append(sdk).Append(roll).ToList()};
  }

  public static DotnetCliCommand ReferenceProject(string referencingProject, string referenceTarget)
  {
    return new DotnetCliCommand {Arguments = new[] {"add", referencingProject, "reference", referenceTarget}};
  }

  public static DotnetCliCommand AddProjectToSolution(string solution, string project)
  {
    return new DotnetCliCommand {Arguments = new[] {"sln", solution, "add", project}};
  }

  public static DotnetCliCommand GetDotnetProjectTemplates()
  {
    return new DotnetCliCommand {Arguments = new[] {"new", "list", "--type", "project", "--columns", "language"}};
  }

  public static DotnetCliCommand NewSolution(string solutionName, string path)
  {
    return new DotnetCliCommand {Arguments = new[] {"new", "sln", "--name", solutionName, "--output", path}};
  }
}
