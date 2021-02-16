using System;
using System.Linq;

namespace Cicee.CiEnv
{
  public record ProjectContinuousIntegrationEnvironmentDefinition
  {
    public ProjectEnvironmentVariable[] Variables { get; init; } =
      Array.Empty<ProjectEnvironmentVariable>();

    public virtual bool Equals(ProjectContinuousIntegrationEnvironmentDefinition? other)
    {
      if (ReferenceEquals(objA: null, other))
      {
        return false;
      }

      if (ReferenceEquals(objA: this, other))
      {
        return true;
      }

      return Variables.SequenceEqual(other.Variables);
    }

    public override int GetHashCode()
    {
      return Variables.GetHashCode();
    }
  }
}
