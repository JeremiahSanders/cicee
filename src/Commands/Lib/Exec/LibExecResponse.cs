using System.Diagnostics;

using Cicee.Dependencies;

namespace Cicee.Commands.Lib.Exec;

public record LibExecResponse(ProcessStartInfo ProcessStartInfo, ProcessExecResult ProcessExecResult)
{
  public LibraryShellTemplate Shell { get; init; } = LibraryShellTemplate.Bash;
}
