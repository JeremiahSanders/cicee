using System.Collections.Generic;

namespace Cicee.Dependencies;

/// <summary>
///   Dependencies which provide access to environment variables and other ambient/installation information.
/// </summary>
public interface IEnvironmentDependencies
{
  /// <summary>Gets the CICEE assembly's <c>lib</c> content directory path.</summary>
  string GetLibraryRootPath();


  /// <summary>Gets the <c>init templates</c> command's directory path.</summary>
  string GetInitTemplatesDirectoryPath();


  /// <summary>Gets the current environment variables.</summary>
  IReadOnlyDictionary<string, string> GetEnvironmentVariables();
}
