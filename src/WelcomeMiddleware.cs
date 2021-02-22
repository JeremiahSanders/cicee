using System;
using System.CommandLine.Invocation;
using System.Reflection;
using System.Threading.Tasks;

namespace Cicee
{
  public static class WelcomeMiddleware
  {
    public static Task InvokeMiddleware(InvocationContext context, Func<InvocationContext, Task> next)
    {
      context.Console.Out.Write(
        $"\n-- cicee (v{typeof(WelcomeMiddleware).Assembly.GetName().Version?.ToString(fieldCount: 3)}) --\n\n"
      );
      return next(context);
    }
  }
}
