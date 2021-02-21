using System;
using System.Collections.Generic;
using Cicee.Commands.Exec;
using LanguageExt;
using LanguageExt.Common;

namespace Cicee.Tests.Unit.Commands.Exec.ExecHandlingTests
{
  public static class ExecHandlingTestHelpers
  {
    public static ExecDependencies CreateDependencies()
    {
      return new(
        dir => new Result<string>(dir),
        file => new Result<string>(file),
        () => new Dictionary<string, string>(),
        () => "/cicee/lib",
        info => new Result<ProcessExecResult>(value: new ProcessExecResult()).AsTask(),
        _ => { },
        _ => { },
        path => new Result<string>(e: new Exception(message: $"Path {path} not arranged."))
      );
    }
  }
}
