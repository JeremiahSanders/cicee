using Cicee.Commands.Meta.Version;
using Xunit;

namespace Cicee.Tests.Unit.Commands.Meta.Version;

public static class MetaVersionCommandTests
{
  public class CreateTests
  {
    [Fact]
    public void WhenExecutedFromChildDirectory_ReturnsExpectedCommand()
    {
      var expected = new CommandValues(
        MetaVersionCommand.CommandName,
        MetaVersionCommand.CommandDescription,
        new[]
        {
          new OptionValues(
            "metadata",
            "Project metadata file path.",
            new[] {"--metadata", "-m"},
            IsRequired: true
          )
        }
      );
      var dependencies = DependencyHelper.CreateMockDependencies();
      var command = MetaVersionCommand.Create(dependencies);

      var actual = CommandValues.FromCommand(command);

      Assert.Equal(expected, actual);
    }
  }
}
