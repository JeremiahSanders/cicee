using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;

namespace Cicee.Tests.Unit.Commands;

public record CommandValues(string Name, string? Description, IReadOnlyList<OptionValues> Options)
{
  public virtual bool Equals(CommandValues? other)
  {
    if (ReferenceEquals(objA: null, other))
    {
      return false;
    }

    if (ReferenceEquals(this, other))
    {
      return true;
    }

    return Name == other.Name && Description == other.Description && Options.SequenceEqual(other.Options);
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(Name, Description, Options);
  }

  public static CommandValues FromCommand(Command command)
  {
    return new CommandValues(
      command.Name,
      command.Description,
      command.Options.Select(OptionValues.FromOption)
        .ToArray()
    );
  }
}
