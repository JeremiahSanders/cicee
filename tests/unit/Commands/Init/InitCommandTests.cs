using Cicee.Commands.Init;
using Xunit;

namespace Cicee.Tests.Unit.Commands.Init;

public static class InitCommandTests
{
  public class Create
  {
    [Fact]
    public void ReturnsExpectedCommand()
    {
      var expected = new CommandValues(
        "init",
        "Initialize project. Creates suggested CICEE files.",
        new[]
        {
          new OptionValues(
            "project-root",
            "Project repository root directory",
            new[] {"--project-root", "-p"},
            IsRequired: true
          ),
          new OptionValues(
            "image",
            "Base CI image for $PROJECT_ROOT/ci/Dockerfile.",
            new[] {"--image", "-i"},
            IsRequired: false
          ),
          new OptionValues(
            "force",
            "Force writing files. Overwrites files which already exist.",
            new[] {"--force", "-f"},
            IsRequired: false
          )
        }
      );
      var actual = CommandValues.FromCommand(InitCommand.Create(DependencyHelper.CreateMockDependencies()));

      Assert.Equal(expected, actual);
    }
  }
}
