using Cicee.Commands.Lib;

namespace Cicee.Commands.Template.Lib;

public record TemplateLibRequest(string ProjectRoot, LibraryShellTemplate? Shell, bool OverwriteFiles);
