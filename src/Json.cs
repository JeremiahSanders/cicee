using System;
using System.Text.Json;
using LanguageExt;
using LanguageExt.Common;

namespace Cicee
{
  public static class Json
  {
    private static readonly JsonSerializerOptions DefaultOptions =
      new() {PropertyNameCaseInsensitive = true};

    public static Result<T> TryDeserialize<T>(string possibleJson)
    {
      return Prelude.Try(() =>
        {
          var deserialized = JsonSerializer.Deserialize<T>(possibleJson, DefaultOptions);
          return deserialized ?? throw new Exception($"Failed to deserialize. Value:\n{possibleJson}");
        })
        .Try()!;
    }

    public static Result<string> TrySerialize<T>(T obj)
    {
      return Prelude.Try(() => JsonSerializer.Serialize(obj)).Try()!;
    }
  }
}
