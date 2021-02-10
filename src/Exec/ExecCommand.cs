using System.CommandLine;
using System.CommandLine.Invocation;

namespace Cicee.Exec
{
  static class ExecCommand
  {
    private static Option CommandOption()
    {
      return new Option<string>(
          new[] { "--command", "-c" },
          "Execution command"
          )
      { IsRequired = false };
    }
    private static Option EntrypointOption()
    {
      return new Option<string?>(
          new[] { "--entrypoint", "-e" },
          "Execution entrypoint"
          )
      { IsRequired = false };
    }
    public static Command Create()
    {

      var command = new Command("exec", "Execute")
          {
              ProjectRootOption.Create(),
              CommandOption(),
              EntrypointOption()
          };
      command.Handler = CommandHandler.Create<string, string?, string?>(ExecHandling.HandleAsync);
      return command;
    }
  }
}
