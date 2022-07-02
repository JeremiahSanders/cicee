using System;
using System.Collections.Generic;

namespace Cicee.Tests.Unit;

public class KeyValuePairValueComparer<Key, Value> : IEqualityComparer<KeyValuePair<Key, Value>>
{
  public bool Equals(KeyValuePair<Key, Value> x, KeyValuePair<Key, Value> y)
  {
    return x.Value != null && x.Key != null && x.Key.Equals(y.Key) && x.Value.Equals(y.Value);
  }

  public int GetHashCode(KeyValuePair<Key, Value> obj)
  {
    return HashCode.Combine(obj.Key, obj.Value);
  }
}
