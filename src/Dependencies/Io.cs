using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using LanguageExt;
using LanguageExt.Common;

namespace Cicee.Dependencies;

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
    return DoesFileExist(file).Bind(
      exists => exists
        ? new Result<string>(file)
        : new Result<string>(new FileNotFoundException($"File '{file}' does not exist.", file))
    );
  }

  /// <summary>
  ///   Helper method which converts Windows paths to Linux style.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Converts Windows drive letters to lower-case top-level directories. E.g., <c>D:\place\thing.txt</c> would become
  ///     <c>/d/place/thing.txt</c>.
  ///   </para>
  /// </remarks>
  /// <param name="path">A path.</param>
  /// <returns>A path which may be Linux-friendly.</returns>
  internal static string NormalizeToLinuxPath(string path)
  {
    return path.Contains(value: ":\\") ? WindowsToLinuxPath(path) : path;

    static string WindowsToLinuxPath(string path)
    {
      string[] driveAndPath = path.Split(separator: ":\\");
      return $"/{driveAndPath[0].ToLowerInvariant()}/{driveAndPath[1].Replace(oldChar: '\\', newChar: '/')}";
    }
  }

  /// <summary>
  ///   Gets the CICEE assembly's <c>lib</c> content directory path.
  /// </summary>
  public static string GetLibraryRootPath()
  {
    string executionPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
    return Path.Combine(executionPath, path2: "lib");
  }

  public static Result<FileCopyRequest> CopyTemplateToPath(FileCopyRequest copyRequest,
    IReadOnlyDictionary<string, string> templateValues)
  {
    return TryCopyTemplateFile(copyRequest.SourcePath, copyRequest.DestinationPath, templateValues)
      .Map(_ => copyRequest);
  }

  public static string GetFileNameForPath(string path)
  {
    return Path.GetFileName(path);
  }

  public static string GetInitTemplatesDirectoryPath()
  {
    string executionPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
    return Path.Combine(executionPath, path2: "templates", path3: "init");
  }

  public static Result<string> TryGetCurrentDirectory()
  {
    return Prelude.Try(Directory.GetCurrentDirectory).Try();
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
    return Prelude.Try(
      () =>
      {
        return Prelude.pipe(
          source,
          File.ReadAllText,
          InterpolateValues,
          contents =>
          {
            CreateDirectoryIfNotExists();
            File.WriteAllText(destination, contents);
            return (source, destination);
          }
        );
      }
    ).Try();

    string InterpolateValues(string content)
    {
      return tokenReplacements.SelectMany(
        kvp => new[]
        {
          new KeyValuePair<string, string>($"<%= {kvp.Key} %>", kvp.Value),
          new KeyValuePair<string, string>($"<%={kvp.Key}%>", kvp.Value),
          new KeyValuePair<string, string>($"<%={kvp.Key} %>", kvp.Value),
          new KeyValuePair<string, string>($"<%= {kvp.Key}%>", kvp.Value)
        }
      ).Fold(content, (latestContent, keyValuePair) => latestContent.Replace(keyValuePair.Key, keyValuePair.Value));
    }

    void CreateDirectoryIfNotExists()
    {
      string? destinationDirectory = Path.GetDirectoryName(destination);
      if (!Directory.Exists(destinationDirectory))
      {
        Directory.CreateDirectory(destinationDirectory!);
      }
    }
  }

  public static Task<Result<(string FileName, string Content)>> TryWriteFileStringAsync(
    (string FileName, string Content) tuple)
  {
    return Prelude.TryAsync(
      async () =>
      {
        await File.WriteAllTextAsync(tuple.FileName, tuple.Content);
        return tuple;
      }
    ).Try();
  }

  public static Task<Result<DirectoryCopyResult>> TryCopyDirectoryAsync(DirectoryCopyRequest request)
  {
    (string sourceDirectory, string destinationDirectory, bool overwrite) = request;

    return Prelude.TryAsync(CopyDirectoryAsync).Try();

    Task<DirectoryCopyResult> CopyDirectoryAsync()
    {
      if (!Directory.Exists(sourceDirectory))
      {
        throw new DirectoryNotFoundException($"Source directory not found. Source: {sourceDirectory}");
      }

      List<(string SourceDirectory, string TargetDirectory)> createdDirectories = new();
      List<(string SourceFile, string TargetFile)> copiedFiles = new();
      List<(string SourceDirectory, string TargetDirectory)> skippedDirectoryCreation = new();
      List<(string SourceFile, string TargetFile)> skippedSourceFiles = new();

      foreach (string subdirectory in Directory.GetDirectories(
                 sourceDirectory,
                 searchPattern: "*",
                 SearchOption.AllDirectories
               ))
      {
        // NOTE: Path.Join used because we must append an arbitrary depth of path components, rather than a single additional component.
        string target = Path.Join(destinationDirectory, subdirectory.Substring(sourceDirectory.Length));

        if (Directory.Exists(target))
        {
          skippedDirectoryCreation.Add((subdirectory, target));
        }
        else
        {
          Directory.CreateDirectory(target);
          createdDirectories.Add((subdirectory, target));
        }
      }

      foreach (string sourceFilePath in Directory.GetFiles(
                 sourceDirectory,
                 searchPattern: "*.*",
                 SearchOption.AllDirectories
               ))
      {
        // NOTE: Path.Join used because we must append an arbitrary depth of path components, rather than a single additional component.
        string targetFilePath = Path.Join(destinationDirectory, sourceFilePath.Substring(sourceDirectory.Length));

        if (overwrite || !File.Exists(targetFilePath))
        {
          File.Copy(sourceFilePath, targetFilePath, overwrite);
          copiedFiles.Add((sourceFilePath, targetFilePath));
        }
        else
        {
          skippedSourceFiles.Add((sourceFilePath, targetFilePath));
        }
      }

      return new DirectoryCopyResult(
        sourceDirectory,
        destinationDirectory,
        overwrite,
        createdDirectories,
        copiedFiles,
        skippedDirectoryCreation,
        skippedSourceFiles
      ).AsTask();
    }
  }

  public static Result<string> TryGetParentDirectory(string path)
  {
    return Prelude.Try(
      () => new DirectoryInfo(path).Parent?.FullName ??
            throw new DirectoryNotFoundException($"No parent found for: {path}")
    ).Try();
  }
}
