using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Cicee.Dependencies;

using LanguageExt;
using LanguageExt.Common;

namespace Cicee.Edges.Filesystem;

public static class FileCopyHelpers
{
  public static Task<Result<IReadOnlyCollection<FileCopyResult>>> TryWriteFiles(
    ICommandDependencies dependencies,
    IReadOnlyCollection<FileCopyRequest> files,
    IReadOnlyDictionary<string, string> templateValues,
    bool overwriteFiles)
  {
    return files
      .Fold(
        new Result<IEnumerable<FileCopyResult>>(Array.Empty<FileCopyResult>()),
        (lastResult, request) => lastResult.Bind(fileCopyResults => TryCopyFile(request)
          .Map(fileCopyResults.Append)
        )
      )
      .Map(static IReadOnlyCollection<FileCopyResult> (results) => results.ToList())
      .AsTask();

    Result<FileCopyResult> TryCopyFile(FileCopyRequest fileCopyRequest)
    {
      return dependencies
        .DoesFileExist(fileCopyRequest.SourcePath)
        .Bind(exists => exists
          ? dependencies.DoesFileExist(fileCopyRequest.DestinationPath)
          : new Result<bool>(
            new FileNotFoundException($"Failed to find template file '{fileCopyRequest.SourcePath}'.")
          )
        )
        .Bind(destinationExists => !destinationExists || overwriteFiles
          ? dependencies
            .CopyTemplateToPath(fileCopyRequest, templateValues)
            .Map(savedRequest => new FileCopyResult(savedRequest, Written: true))
          : new Result<FileCopyResult>(new FileCopyResult(fileCopyRequest, Written: false))
        );
    }
  }
}
