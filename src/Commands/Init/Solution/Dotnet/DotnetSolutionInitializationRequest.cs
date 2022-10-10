using Cicee.Commands.Init.Solution.Options;

namespace Cicee.Commands.Init.Solution.Dotnet;

public record DotnetSolutionInitializationRequest
{
  /// <summary>
  ///   Gets the solution's (repository project's / application's) name.
  /// </summary>
  public string SolutionName { get; init; } = string.Empty;

  public DotnetLanguageOption.DotnetLanguage Language { get; init; } = DotnetLanguageOption.DotnetLanguage.CSharp;

  /// <summary>
  ///   Gets the .NET namespace in which the solution's .NET projects are created.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Use this to specify organization or meta-project namespace.
  ///   </para>
  ///   <para>Example: An application</para>
  /// </remarks>
  public string? DotnetNamespacePrefix { get; init; }

  public string ProjectRoot { get; init; } = string.Empty;

  public DotnetProjectParameters Application { get; init; } = new();
  public DotnetProjectParameters? Domain { get; init; }
  public DotnetProjectParameters? UnitTests { get; init; }
  public DotnetProjectParameters? IntegrationTests { get; init; }
}
