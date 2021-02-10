using System;
using Nemesis.Essentials.Design;

namespace Cicee
{
  public record ProjectContinuousIntegrationEnvironmentDefinition
  {
    public ValueCollection<ProjectEnvironmentVariable> Variables { get; init; } =
      new(Array.Empty<ProjectEnvironmentVariable>());
  }
}
