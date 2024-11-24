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
      CommandValues expected = new(
        Name: "init",
        Description: "Initialize project. Creates suggested CICEE files.",
        new[]
        {
          new OptionValues(
            Name: "project-root",
            Description: "Project repository root directory",
            new[]
            {
              "--project-root",
              "-p"
            },
            IsRequired: true
          ),
          new OptionValues(
            Name: "image",
            Description: "Base CI image for $PROJECT_ROOT/ci/Dockerfile.",
            new[]
            {
              "--image",
              "-i"
            },
            IsRequired: false
          ),
          new OptionValues(
            Name: "force",
            Description: "Force writing files. Overwrites files which already exist.",
            new[]
            {
              "--force",
              "-f"
            },
            IsRequired: false
          )
        }
      );
      CommandValues actual = CommandValues.FromCommand(InitCommand.Create(DependencyHelper.CreateMockDependencies()));

      Assert.Equal(expected, actual);
    }
  }
}
