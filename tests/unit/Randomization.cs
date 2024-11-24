using System;

namespace Cicee.Tests.Unit;

public static class Randomization
{
  private static Random Rng { get; } = Random.Shared;

  public static bool Boolean()
  {
    return Rng.Boolean();
  }

  public static bool Boolean(this Random random)
  {
    return random.Next(maxValue: 2) % 2 == 0;
  }

  public static byte Byte()
  {
    return Rng.Byte();
  }

  public static byte Byte(this Random random)
  {
    return Convert.ToByte(random.Next(minValue: 0, maxValue: 256));
  }

  public static string GuidString()
  {
    return Guid.NewGuid().ToString(format: "D");
  }
}
