using System.Diagnostics;

namespace Cicee.Commands.Init.Solution.Dotnet.PlannedInitializers;

public interface ICreateProcessStartInfo
{
  ProcessStartInfo ToProcessStartInfo();
}
