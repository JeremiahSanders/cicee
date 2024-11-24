using System;
using System.Collections.Generic;

namespace Cicee.Tests.Unit;

public class KeyValuePairValueComparer<TKey, TValue> : IEqualityComparer<KeyValuePair<TKey, TValue>>
{
  public bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
  {
    return x.Value != null && x.Key != null && x.Key.Equals(y.Key) && x.Value.Equals(y.Value);
  }

  public int GetHashCode(KeyValuePair<TKey, TValue> obj)
  {
    return HashCode.Combine(obj.Key, obj.Value);
  }
}
