namespace Cicee.Commands.Init
{
  public record InitRequest(string ProjectRoot, string? Image, bool OverwriteFiles);
}
