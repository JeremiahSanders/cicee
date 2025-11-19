using System;

namespace Cicee.Dependencies;

/// <summary>
///   Dependencies which provide access to the console.
/// </summary>
public interface IConsoleDependencies
{
  /// <summary>Writes to standard out (or the conceptual equivalent used by the implementation).</summary>
  void StandardOutWrite(ConsoleColor? color, string text);

  /// <summary>
  ///   Writes <paramref name="text" /> to standard out (or the conceptual equivalent used by the implementation) with
  ///   a trailing <see cref="Environment.NewLine" />.
  /// </summary>
  void StandardOutWriteLine(string text);

  /// <summary>
  ///   Writes <paramref name="text" /> to standard error (or the conceptual equivalent used by the implementation)
  ///   with a trailing <see cref="Environment.NewLine" />.
  /// </summary>
  void StandardErrorWriteLine(string text);
}
