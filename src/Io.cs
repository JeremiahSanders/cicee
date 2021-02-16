using System.IO;
using LanguageExt;
using LanguageExt.Common;

namespace Cicee
{
  public static class Io
  {
    public static Result<string> EnsureDirectoryExists(string directory)
    {
      return Directory.Exists(directory)
        ? new Result<string>(directory)
        : new Result<string>(e: new DirectoryNotFoundException(message: $"Directory '{directory}' does not exist."));
    }

    public static Result<string> EnsureFileExists(string file)
    {
      return File.Exists(file)
        ? new Result<string>(file)
        : new Result<string>(e: new FileNotFoundException(message: $"File '{file}' does not exist.", file));
    }

    public static Result<string> TryLoadFileString(string file)
    {
      return Prelude.Try(() => File.ReadAllText(file)).Try();
    }
  }
}
