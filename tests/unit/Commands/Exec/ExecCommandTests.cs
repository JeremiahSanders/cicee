using Cicee.Commands.Exec;
using Xunit;

namespace Cicee.Tests.Unit.Commands.Exec
{
  public static class ExecCommandTests
  {
    public class Create
    {
      [Fact]
      public void ReturnsExpectedCommand()
      {
        var expected = new CommandValues(
          Name: "exec",
          Description: "Execute",
          Options: new[]
          {
            new OptionValues(
              Name: "project-root",
              Description: "Project repository root directory",
              Aliases: new[] {"--project-root", "-p"},
              IsRequired: true
            ),
            new OptionValues(
              Name: "command",
              Description: "Execution command",
              Aliases: new[] {"--command", "-c"},
              IsRequired: false
            ),
            new OptionValues(
              Name: "entrypoint",
              Description: "Execution entrypoint",
              Aliases: new[] {"--entrypoint", "-e"},
              IsRequired: false
            )
          }
        );

        var actual = CommandValues.FromCommand(command: ExecCommand.Create());

        Assert.Equal(expected, actual);
      }
    }
  }
}
