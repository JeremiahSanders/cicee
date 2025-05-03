using System.Diagnostics;

using Cicee.Dependencies;
using Cicee.Edges.Processes;

namespace Cicee.Commands.Lib.Exec;

public record LibExecResponse(ProcessExecRequest ProcessExecRequest, ProcessExecResult ProcessExecResult)
{
  public LibraryShellTemplate Shell { get; init; } = LibraryShellTemplate.Bash;
}
