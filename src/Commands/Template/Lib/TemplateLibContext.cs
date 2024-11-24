using Cicee.Commands.Lib;

namespace Cicee.Commands.Template.Lib;

public record TemplateLibContext(
  string ProjectRoot,
  LibraryShellTemplate ShellTemplate,
  string LibPath,
  bool OverwriteFiles);
