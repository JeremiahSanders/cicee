namespace Cicee.Dependencies;

/// <summary>
///   Dependencies (e.g., I/O) required for command processing.
/// </summary>
/// <remarks>This is an aggregate interface for command dependencies.</remarks>
public interface ICommandDependencies : IPathDependencies, IDirectoryDependencies, IFileDependencies,
  IConsoleDependencies, IEnvironmentDependencies, IProcessDependencies
{
}
