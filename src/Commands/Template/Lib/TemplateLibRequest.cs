namespace Cicee.Commands.Template.Lib
{
  public record TemplateLibRequest(
    string ProjectRoot,
    string? Shell,
    bool OverwriteFiles
  );
}
