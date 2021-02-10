namespace Cicee.Exec
{
  public record ExecContext(string ProjectRoot, ProjectMetadata ProjectMetadata, string? Command, string? Entrypoint);
}
