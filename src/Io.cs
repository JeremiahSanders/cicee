using System.Collections.Generic;
using System.IO;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;

namespace Cicee
{
  public static class Io
  {
    public static Result<bool> DoesFileExist(string file)
    {
      return Prelude.Try(() => File.Exists(file)).Try();
    }

    public static Result<string> EnsureDirectoryExists(string directory)
    {
      return Directory.Exists(directory)
        ? new Result<string>(directory)
        : new Result<string>(new DirectoryNotFoundException($"Directory '{directory}' does not exist."));
    }

    public static Result<string> EnsureFileExists(string file)
    {
      return DoesFileExist(file)
        .Bind(exists =>
          exists
            ? new Result<string>(file)
            : new Result<string>(new FileNotFoundException($"File '{file}' does not exist.", file))
        );
    }

    public static Result<string> TryLoadFileString(string file)
    {
      return Prelude.Try(() => File.ReadAllText(file)).Try();
    }

    public static string PathCombine2(string path1, string path2)
    {
      return Path.Combine(path1, path2);
    }

    public static string PathCombine3(string path1, string path2, string path3)
    {
      return Path.Combine(path1, path2, path3);
    }

    public static Result<(string Source, string Destination)> TryCopyTemplateFile(string source, string destination,
      IReadOnlyDictionary<string, string> tokenReplacements)
    {
      string InterpolateValues(string content)
      {
        return tokenReplacements.SelectMany(kvp => new[]
          {
            new KeyValuePair<string, string>($"<%= {kvp.Key} %>", kvp.Value),
            new KeyValuePair<string, string>($"<%={kvp.Key}%>", kvp.Value),
            new KeyValuePair<string, string>($"<%={kvp.Key} %>", kvp.Value),
            new KeyValuePair<string, string>($"<%= {kvp.Key}%>", kvp.Value)
          })
          .Fold(content,
            (latestContent, keyValuePair) => latestContent.Replace(keyValuePair.Key, keyValuePair.Value)
          );
      }

      return Prelude.Try(() =>
      {
        return Prelude.pipe(
          source,
          File.ReadAllText,
          InterpolateValues,
          contents =>
        {
          File.WriteAllText(destination, contents);
          return (source, destination);
        });
      }).Try();
    }
  }
}
