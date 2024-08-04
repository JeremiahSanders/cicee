using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;

namespace Cicee.Tests.Unit.Commands;

public record OptionValues(string Name, string? Description, IReadOnlyCollection<string> Aliases, bool IsRequired)
{
  public virtual bool Equals(OptionValues? other)
  {
    if (ReferenceEquals(objA: null, other))
    {
      return false;
    }

    if (ReferenceEquals(this, other))
    {
      return true;
    }

    return Name == other.Name && Description == other.Description && Aliases.SequenceEqual(other.Aliases) &&
           IsRequired == other.IsRequired;
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(Name, Description, Aliases, IsRequired);
  }

  public static OptionValues FromOption(Option option)
  {
    return new OptionValues(option.Name, option.Description, option.Aliases, option.IsRequired);
  }
}
