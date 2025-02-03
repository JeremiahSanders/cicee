using System.Threading.Tasks;

using Cicee.Commands.Exec.Handling;
using Cicee.Dependencies;

using LanguageExt;
using LanguageExt.Common;

namespace Cicee.Commands.Exec;

public class ExecHandler
{
  private readonly ICommandDependencies _dependencies;

  public ExecHandler(ICommandDependencies dependencies)
  {
    _dependencies = dependencies;
  }

  public Task<Result<ExecResult>> HandleAsync(ExecRequest request)
  {
    return HandleAsync(_dependencies, request);
  }

  public static async Task<Result<ExecResult>> HandleAsync(ICommandDependencies dependencies, ExecRequest request)
  {
    DisplayRequest(dependencies, request);

    return (await IoContext
      .TryCreateRequestContext(dependencies, request)
      .Bind(execContext => IoContext.ValidateContext(dependencies, execContext))
      .Map(context => IoContext.DisplayExecContext(dependencies, context))
      .BindAsync(execContext => TryExecute(dependencies, execContext))).Map(context => new ExecResult(request));
  }

  private static void DisplayRequest(ICommandDependencies dependencies, ExecRequest request)
  {
    dependencies.StandardOutWriteLine(text: "Beginning exec...\n");
    dependencies.StandardOutWriteLine($"Project root: {request.ProjectRoot}");
    dependencies.StandardOutWriteLine($"Entrypoint  : {request.Entrypoint}");
    dependencies.StandardOutWriteLine($"Command     : {request.Command}");
  }

  private static Task<Result<ExecRequestContext>> TryExecute(
    ICommandDependencies dependencies,
    ExecRequestContext execRequestContext)
  {
    return execRequestContext.Harness is ExecInvocationHarness.Direct ? HandleDirectAsync() : HandleScriptAsync();

    async Task<Result<ExecRequestContext>> HandleScriptAsync()
    {
      return (await ScriptHarness
        .CreateProcessStartInfo(dependencies, execRequestContext)
        .BindAsync(dependencies.ProcessExecutor)).Map(_ => execRequestContext);
    }

    async Task<Result<ExecRequestContext>> HandleDirectAsync()
    {
      return await Prelude
        .TryAsync(() => DirectHarness.InvokeDockerCommandsAsync(dependencies, execRequestContext))
        .Try();
    }
  }
}
