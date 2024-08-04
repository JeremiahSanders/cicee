using System;
using System.Text.Encodings.Web;
using System.Text.Json;

using LanguageExt;
using LanguageExt.Common;

namespace Cicee.Dependencies;

public static class Json
{
  /// <summary>
  ///   Default options used for (de)serializing JSON.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Uses <see cref="JavaScriptEncoder.UnsafeRelaxedJsonEscaping" /> as its
  ///     <see cref="JsonSerializerOptions.Encoder" /> to avoid changing non-cicee values in project metadata (i.e., when
  ///     using <c>package.json</c>).
  ///   </para>
  /// </remarks>
  internal static JsonSerializerOptions DefaultOptions { get; } = new()
  {
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
  };

  public static Result<T> TryDeserialize<T>(string possibleJson)
  {
    return Prelude.Try(
      () =>
      {
        T? deserialized = JsonSerializer.Deserialize<T>(possibleJson, DefaultOptions);
        return deserialized ?? throw new Exception($"Failed to deserialize. Value:\n{possibleJson}");
      }
    ).Try()!;
  }

  public static Result<string> TrySerialize<T>(T obj)
  {
    return TrySerialize(obj, DefaultOptions);
  }

  public static Result<string> TrySerialize<T>(T obj, JsonSerializerOptions options)
  {
    return Prelude.Try(() => JsonSerializer.Serialize(obj, options)).Try()!;
  }
}
