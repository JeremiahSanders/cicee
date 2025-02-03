using System.Collections.Generic;
using System.Diagnostics;

namespace Cicee.Dependencies;

public static class ProcessExecRequestExtensions
{
  /// <summary>
  ///   Converts this <see cref="ProcessExecRequest" /> into a <see cref="ProcessStartInfo" />.
  /// </summary>
  /// <param name="request"></param>
  /// <returns></returns>
  public static ProcessStartInfo ToProcessStartInfo(this ProcessExecRequest request)
  {
    return ConstructProcessStartInfoWithoutEnvironment(request)
      .ApplyEnvironment(request);
  }

  private static ProcessStartInfo ConstructProcessStartInfoWithoutEnvironment(ProcessExecRequest request)
  {
    return request switch
    {
      {FileName: not null, Arguments: not null} => new ProcessStartInfo(request.FileName, request.Arguments)
      {
        UseShellExecute = request.UseShellExecute, WorkingDirectory = request.WorkingDirectory
      },
      {FileName: not null, Arguments: null} => new ProcessStartInfo(request.FileName)
      {
        UseShellExecute = request.UseShellExecute, WorkingDirectory = request.WorkingDirectory
      },
      _ => new ProcessStartInfo
      {
        UseShellExecute = request.UseShellExecute, WorkingDirectory = request.WorkingDirectory
      }
    };
  }

  private static ProcessStartInfo ApplyEnvironment(this ProcessStartInfo info, ProcessExecRequest request)
  {
    foreach (KeyValuePair<string, string?> kvp in request.Environment)
    {
      info.Environment[kvp.Key] = kvp.Value;
    }

    return info;
  }
}
