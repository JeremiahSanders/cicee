using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace Cicee.Commands.Init
{
  public static class FileCopyHelpers
  {
    public static Task<Result<IReadOnlyCollection<FileCopyResult>>> TryWriteFiles(
      CommandDependencies dependencies,
      IReadOnlyCollection<FileCopyRequest> files,
      IReadOnlyDictionary<string, string> templateValues,
      bool overwriteFiles
    )
    {
      Result<FileCopyResult> TryCopyFile(FileCopyRequest fileCopyRequest)
      {
        return dependencies.DoesFileExist(fileCopyRequest.SourcePath)
          .Bind(exists =>
            exists
              ? dependencies.DoesFileExist(fileCopyRequest.DestinationPath)
              : new Result<bool>(
                new FileNotFoundException($"Failed to find template file '{fileCopyRequest.SourcePath}'.")
              )
          )
          .Bind(destinationExists =>
            !destinationExists || overwriteFiles
              ? dependencies.CopyTemplateToPath(fileCopyRequest, templateValues)
                .Map(savedRequest => new FileCopyResult(savedRequest, Written: true))
              : new Result<FileCopyResult>(new FileCopyResult(fileCopyRequest, Written: false))
          );
      }

      return files
        .Fold(
          new Result<IEnumerable<FileCopyResult>>(Array.Empty<FileCopyResult>()),
          (lastResult, request) =>
            lastResult.Bind(fileCopyResults =>
              TryCopyFile(request)
                .Map(fileCopyResults.Append)
            )
        )
        .Map(results => (IReadOnlyCollection<FileCopyResult>)results.ToList())
        .AsTask();
    }
  }
}
