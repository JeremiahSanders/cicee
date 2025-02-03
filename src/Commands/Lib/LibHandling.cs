using System.Threading.Tasks;

using Cicee.Dependencies;

using LanguageExt.Common;

namespace Cicee.Commands.Lib;

public static class LibHandling
{
  public static async Task<Result<LibContext>> HandleAsync(ICommandDependencies dependencies, LibRequest libRequest)
  {
    return (await Validation.ValidateRequestAsync(dependencies, libRequest)).TapSuccess(
      context => { dependencies.StandardOutWriteLine(context.LibPath); }
    );
  }
}
