namespace Cicee.Commands.Template.Init
{
  public record TemplateInitRequest(
    string ProjectRoot,
    bool OverwriteFiles,
    string? MetadataFile = null,
    string? Name = null,
    string? Title = null,
    string? Description = null,
    string? Version = null
  );
}
