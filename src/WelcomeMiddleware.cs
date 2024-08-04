using System;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading.Tasks;

namespace Cicee;

public static class WelcomeMiddleware
{
  public static Task InvokeMiddleware(InvocationContext context, Func<InvocationContext, Task> next)
  {
    if (!RequiresRawOutput(context))
    {
      context.Console.Out.Write(
        $"\n-- cicee (v{typeof(WelcomeMiddleware).Assembly.GetName().Version?.ToString(fieldCount: 3)}) --\n\n"
      );
    }

    return next(context);
  }


  private static bool RequiresRawOutput(InvocationContext context)
  {
    return IsLib() || IsMetaVersion();

    bool IsLib()
    {
      return context.ParseResult.Tokens.FirstOrDefault()?.Value == "lib";
    }

    bool IsMetaVersion()
    {
      return context.ParseResult.Tokens.Select(token => token.Value).Take(count: 2).SequenceEqual(
        new[]
        {
          "meta",
          "version"
        }
      );
    }
  }
}
