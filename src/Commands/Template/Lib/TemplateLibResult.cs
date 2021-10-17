using System.Collections.Generic;
using Cicee.Commands.Lib;

namespace Cicee.Commands.Template.Lib
{
  public record TemplateLibResult(
    string ProjectRoot,
    LibraryShellTemplate ShellTemplate,
    string LibPath,
    bool OverwriteFiles,
    DirectoryCopyResult CiLibraryCopyResult,
    IReadOnlyCollection<FileCopyResult> CiceeExecCopyResults,
    string CiLibraryEntrypointPath,
    string CiceeExecEntrypointPath
  );
}
