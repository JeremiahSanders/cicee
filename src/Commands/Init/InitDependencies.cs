using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using LanguageExt.Common;

namespace Cicee.Commands.Init
{
  [ExcludeFromCodeCoverage]
  public record InitDependencies
  (
    Func<string, string, string> CombinePath,
    Func<FileCopyRequest, IReadOnlyDictionary<string, string>, Result<FileCopyRequest>> CopyTemplateToPath,
    Func<string, Result<bool>> DoesFileExist,
    Func<string, Result<string>> EnsureDirectoryExists,
    Func<string> GetInitTemplatesDirectoryPath,
    Action<string> WriteInformation
  )
  {
    public static InitDependencies Create()
    {
      static Result<FileCopyRequest> CopyTemplateToPath(FileCopyRequest copyRequest, IReadOnlyDictionary<string, string> templateValues)
      {
        return Io.TryCopyTemplateFile(copyRequest.SourcePath, copyRequest.DestinationPath, templateValues).Map(_ => copyRequest);
      }

      static string GetInitTemplatesDirectoryPath()
      {
        var executionPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
        return Path.Combine(executionPath, "templates", "init");
      }

      return new InitDependencies(
        Io.PathCombine2,
        CopyTemplateToPath,
        Io.DoesFileExist,
        Io.EnsureDirectoryExists,
        GetInitTemplatesDirectoryPath,
        Console.WriteLine
      );
    }
  }
}
