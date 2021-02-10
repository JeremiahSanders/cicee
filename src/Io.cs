using System.IO;
using Cicee.Exec;
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
        : new Result<string>(new BadRequestException($"Directory '{directory}' does not exist."));
    }

    public static Result<string> EnsureFileExists(string file)
    {
      return File.Exists(file)
        ? new Result<string>(file)
        : new Result<string>(new BadRequestException($"File '{file}' does not exist."));
    }

    public static Result<string> TryLoadFileString(string file)
    {
      return Prelude.Try(() => File.ReadAllText(file)).Try();
    }
  }
}
