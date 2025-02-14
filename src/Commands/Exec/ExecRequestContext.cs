using System;
using System.Collections.Generic;
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
  string? Image,
  ExecInvocationHarness Harness,
  ExecVerbosity Verbosity,
  string? CiDirectory,
  IReadOnlyList<string> DockerComposeFiles,
  string? LibRoot,
  string CiDockerfileImageTag
)
{
  /// <summary>
  ///   Gets a value indicating whether Docker commands issued by <c>direct</c> harness have a "quiet" arguments passed.
  /// </summary>
  public bool DockerQuiet => Verbosity switch
  {
    ExecVerbosity.Normal => false,
    ExecVerbosity.Quiet => true,
    ExecVerbosity.Verbose => false,
    _ => throw new ArgumentOutOfRangeException()
  };

  /// <summary>
  ///   Gets a value indicating whether debug logs are emitted by <c>direct</c> harness.
  /// </summary>
  public bool WorkflowQuiet => Verbosity switch
  {
    ExecVerbosity.Normal => true,
    ExecVerbosity.Quiet => true,
    ExecVerbosity.Verbose => false,
    _ => throw new ArgumentOutOfRangeException()
  };
}
