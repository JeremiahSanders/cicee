using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
  );
}
