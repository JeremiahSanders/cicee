using Cicee.Commands.Template.Init;
using Cicee.Commands.Template.Lib;

namespace Cicee.Commands.Init.Repository;

public record RepositoryResult(InitRequest InitResult, TemplateInitResult TemplateInitResult)
{
  public TemplateLibResult? TemplateLibResult { get; init; }
}
