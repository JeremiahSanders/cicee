using System.CommandLine;

using Cicee.Commands.Meta.Version;
using Cicee.Dependencies;

using Xunit;

namespace Cicee.Tests.Unit.Commands.Meta.Version;

public static class MetaVersionCommandTests
{
  public class CreateTests
  {
    [Fact]
    public void WhenExecutedFromChildDirectory_ReturnsExpectedCommand()
    {
      CommandValues expected = new(
        MetaVersionCommand.CommandName,
        MetaVersionCommand.CommandDescription,
        new[]
        {
          new OptionValues(
            Name: "metadata",
            Description: "Project metadata file path.",
            new[]
            {
              "--metadata",
              "-m"
            },
            IsRequired: true
          )
        }
      );
      CommandDependencies dependencies = DependencyHelper.CreateMockDependencies();
      Command command = MetaVersionCommand.Create(dependencies);

      CommandValues actual = CommandValues.FromCommand(command);

      Assert.Equal(expected, actual);
    }
  }
}
