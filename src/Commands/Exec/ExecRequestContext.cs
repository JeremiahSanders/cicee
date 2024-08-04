using System.Diagnostics.CodeAnalysis;

using Cicee.CiEnv;

namespace Cicee.Commands.Exec;

[ExcludeFromCodeCoverage]
public record ExecRequestContext(
  string ProjectRoot,
  ProjectMetadata ProjectMetadata,
  string? Command,
  string? Entrypoint,
  string? Dockerfile,
  string? Image);
