using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using LanguageExt.Common;

namespace Cicee.Commands.Init
{
  public static class InitEntrypoint
  {
    public static async Task<int> HandleAsync(string projectRoot, string? image, bool force)
    {
      var dependencies = CreateDependencies();
      return (await InitHandling.TryCreateRequest(dependencies, projectRoot, image, force)
          .BindAsync(request => InitHandling.TryHandleRequest(dependencies, request))
        )
        .ToExitCode();
    }

    private static InitDependencies CreateDependencies()
    {
      static Result<FileCopyRequest> CopyTemplateToPath(FileCopyRequest copyRequest, IReadOnlyDictionary<string,string> templateValues)
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
