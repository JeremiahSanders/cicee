using System;
using System.Collections.Generic;

namespace Cicee.Dependencies;

public static class ConsoleDependenciesExtensions
{
  public static void StandardOutWriteAll(
    this IConsoleDependencies commandDependencies,
    IEnumerable<(ConsoleColor? OptionalColor, string Value)> items)
  {
    foreach ((ConsoleColor? OptionalColor, string Value) tuple in items)
    {
      commandDependencies.StandardOutWrite(tuple.OptionalColor, tuple.Value);
    }
  }

  public static void StandardOutWriteAsLine(
    this IConsoleDependencies commandDependencies,
    IEnumerable<(ConsoleColor? OptionalColor, string Value)> items)
  {
    commandDependencies.StandardOutWriteAll(items);
    commandDependencies.StandardOutWrite(color: null, Environment.NewLine);
  }

  public static void LogDebug(this IConsoleDependencies commandDependencies, string message, ConsoleColor? color = null)
  {
    commandDependencies.StandardOutWriteAsLine(
      new[]
      {
        ((ConsoleColor?)(color ?? ConsoleColor.Magenta), message)
      }
    );
  }
}
