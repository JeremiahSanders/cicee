using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using Cicee.Commands.Meta.Version.Bump;
using Cicee.Dependencies;

using LanguageExt.Common;

namespace Cicee.CiEnv;

public static class ProjectMetadataManipulation
{
  private const string ExpectedCiEnvVariablesNodeName = "variables";
  private const string ExpectedCiEnvNodeName = "ciEnvironment";

  /// <summary>
  ///   Replaces the current <see cref="ProjectMetadata.Version" /> in project metadata. Writes <paramref name="version" />
  ///   in SemVer <c>Major.Minor.Patch</c> format.
  /// </summary>
  /// <param name="dependencies"></param>
  /// <param name="projectMetadataPath"></param>
  /// <param name="version"></param>
  /// <returns></returns>
  public static async Task<Result<Version>> UpdateVersionInMetadata(
    ICommandDependencies dependencies,
    string projectMetadataPath,
    Version version)
  {
    return (await ModifyMetadataJson(
      dependencies,
      projectMetadataPath,
      jsonObject =>
      {
        jsonObject[propertyName: "version"] = version.GetVersionString();

        return jsonObject;
      }
    )).Map(_ => version);
  }

  /// <summary>
  ///   Replaces the current <see cref="ProjectContinuousIntegrationEnvironmentDefinition.Variables" /> in project metadata.
  /// </summary>
  /// <param name="dependencies"></param>
  /// <param name="projectMetadataPath"></param>
  /// <param name="updatedVariables"></param>
  /// <returns></returns>
  public static async Task<Result<ProjectEnvironmentVariable[]>> UpdateVariablesInMetadata(
    ICommandDependencies dependencies,
    string projectMetadataPath,
    ProjectEnvironmentVariable[] updatedVariables)
  {
    return (await ModifyMetadataJson(
      dependencies,
      projectMetadataPath,
      jsonObject => UpsertCiEnvNode(
        jsonObject,
        ciEnv => UpsertVariablesNode(ciEnv, currentVariables => VariablesToJsonArray(updatedVariables))
      )
    )).Map(_ => updatedVariables);
  }

  private static JsonNode UpsertVariablesNode(JsonNode ciEnvNode, Func<JsonNode, JsonNode> variablesMutator)
  {
    string variablesNodeName = GetCiEnvVariablesNodeName(ciEnvNode);
    JsonObject envObject = ciEnvNode.AsObject();
    JsonNode variables = envObject.ContainsKey(variablesNodeName) ? envObject[variablesNodeName]! : new JsonArray();
    JsonNode variablesNode = variablesMutator(variables);
    ciEnvNode[variablesNodeName] = variablesNode;

    return ciEnvNode;
  }

  private static JsonObject UpsertCiEnvNode(JsonObject rootJsonObject, Func<JsonNode, JsonNode> ciEnvNodeMutator)
  {
    string ciEnvNodeName = GetCiEnvNodeName(rootJsonObject);
    JsonNode ciEnvNode = rootJsonObject.ContainsKey(ciEnvNodeName) ? rootJsonObject[ciEnvNodeName]! : new JsonObject();
    ciEnvNode = ciEnvNodeMutator(ciEnvNode);
    rootJsonObject[ciEnvNodeName] = ciEnvNode;

    return rootJsonObject;
  }

  private static async Task<Result<(string FileName, string Content, JsonObject MetadataJson)>> ModifyMetadataJson(
    ICommandDependencies dependencies,
    string projectMetadataPath,
    Func<JsonObject, JsonObject> mutator)
  {
    return await dependencies
      .TryLoadFileString(projectMetadataPath)
      .MapSafe(content => JsonNode.Parse(content)!.AsObject())
      .MapSafe(mutator)
      .BindAsync(
        async metadataJson => await Json
          .TrySerialize(metadataJson)
          .BindAsync(
            async jsonString =>
              (await dependencies.TryWriteFileStringAsync((projectMetadataPath, jsonString))).Map(
                tuple => (tuple.FileName, tuple.Content, metadataJson)
              )
          )
      );
  }

  private static JsonArray VariablesToJsonArray(IEnumerable<ProjectEnvironmentVariable> updatedVariables)
  {
    return new JsonArray(
      updatedVariables
        .Select(variable => new JsonObject(GetProperties(variable)))
        .Cast<JsonNode>()
        .ToArray()
    );
  }

  private static IEnumerable<KeyValuePair<string, JsonNode?>> GetProperties(ProjectEnvironmentVariable variable)
  {
    if (variable.DefaultValue != null)
    {
      yield return new KeyValuePair<string, JsonNode?>(key: "defaultValue", variable.DefaultValue);
    }

    yield return new KeyValuePair<string, JsonNode?>(key: "description", variable.Description);
    yield return new KeyValuePair<string, JsonNode?>(key: "name", variable.Name);
    yield return new KeyValuePair<string, JsonNode?>(key: "required", variable.Required);
    yield return new KeyValuePair<string, JsonNode?>(key: "secret", variable.Secret);
  }

  private static string GetCiEnvVariablesNodeName(JsonNode ciEnvironmentNode)
  {
    JsonObject nodeObject = ciEnvironmentNode.AsObject();

    return nodeObject
      .Select(item => item.Key)
      .FirstOrDefault(
        key => key.Equals(ExpectedCiEnvVariablesNodeName, StringComparison.InvariantCultureIgnoreCase)
      ) ?? ExpectedCiEnvVariablesNodeName;
  }

  private static string GetCiEnvNodeName(JsonObject jsonObject)
  {
    return jsonObject
      .Select(item => item.Key)
      .FirstOrDefault(
        item => item.Equals(ExpectedCiEnvNodeName, StringComparison.InvariantCultureIgnoreCase)
      ) ?? ExpectedCiEnvNodeName;
  }
}
