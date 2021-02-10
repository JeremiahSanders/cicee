using System;
using System.Text.Json;
using LanguageExt;
using LanguageExt.Common;

namespace Cicee
{
  public static class Json
  {
    public static Result<T> TryDeserialize<T>(string possibleJson)
    {
      return Prelude.Try(() =>
        {
          var deserialized = JsonSerializer.Deserialize<T>(possibleJson,
            new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
          return deserialized ?? throw new Exception($"Failed to deserialize. Value:\n{possibleJson}");
        })
        .Try()!;
    }
  }
}
