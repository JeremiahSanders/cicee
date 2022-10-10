namespace Cicee.Commands.Init.Solution.Dotnet;

public record DotnetProjectParameters
{
  public string DotnetTemplate { get; init; } = "classlib";
  public string? Framework { get; init; } = string.Empty;
  public string AssemblyName { get; init; } = string.Empty;
  public string Directory { get; init; } = string.Empty;
}
