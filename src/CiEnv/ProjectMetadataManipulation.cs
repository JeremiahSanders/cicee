using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Cicee.Dependencies;
using LanguageExt.Common;

namespace Cicee.CiEnv;

public static class ProjectMetadataManipulation
{
  private const string ExpectedCiEnvVariablesNodeName = "variables";
  private const string ExpectedCiEnvNodeName = "ciEnvironment";

  /// <summary>
  ///   Replaces the current <see cref="ProjectContinuousIntegrationEnvironmentDefinition.Variables" /> in project metadata.
  /// </summary>
  /// <param name="dependencies"></param>
  /// <param name="projectMetadataPath"></param>
  /// <param name="updatedVariables"></param>
  /// <returns></returns>
  public static async Task<Result<ProjectEnvironmentVariable[]>> UpdateVariablesInMetadata(
    CommandDependencies dependencies,
    string projectMetadataPath,
    ProjectEnvironmentVariable[] updatedVariables
  )
  {
    return (await ModifyMetadataJson(dependencies, projectMetadataPath,
          jsonObject => UpsertCiEnvNode(jsonObject,
            ciEnv => UpsertVariablesNode(ciEnv, currentVariables => VariablesToJsonArray(updatedVariables))
          )
        )
      )
      .Map(_ => updatedVariables);
  }

  private static JsonNode UpsertVariablesNode(JsonNode ciEnvNode, Func<JsonNode, JsonNode> variablesMutator)
  {
    var variablesNodeName = GetCiEnvVariablesNodeName(ciEnvNode);
    var envObject = ciEnvNode.AsObject();
    var variables = envObject.ContainsKey(variablesNodeName) ? envObject[variablesNodeName]! : new JsonArray();
    var variablesNode = variablesMutator(variables);
    ciEnvNode[variablesNodeName] = variablesNode;
    return ciEnvNode;
  }

  private static JsonObject UpsertCiEnvNode(JsonObject rootJsonObject, Func<JsonNode, JsonNode> ciEnvNodeMutator)
  {
    var ciEnvNodeName = GetCiEnvNodeName(rootJsonObject);
    var ciEnvNode = rootJsonObject.ContainsKey(ciEnvNodeName) ? rootJsonObject[ciEnvNodeName]! : new JsonObject();
    ciEnvNode = ciEnvNodeMutator(ciEnvNode);
    rootJsonObject[ciEnvNodeName] = ciEnvNode;
    return rootJsonObject;
  }

  private static async Task<Result<(string FileName, string Content, JsonObject MetadataJson)>> ModifyMetadataJson(
    CommandDependencies dependencies,
    string projectMetadataPath,
    Func<JsonObject, JsonObject> mutator)
  {
    return await dependencies.TryLoadFileString(projectMetadataPath)
      .MapSafe(content => JsonNode.Parse(content)!.AsObject())
      .MapSafe(mutator)
      .BindAsync(async metadataJson =>
        await Json.TrySerialize(metadataJson)
          .BindAsync(
            async jsonString =>
              (await dependencies.TryWriteFileStringAsync((projectMetadataPath, jsonString)))
              .Map(tuple => (tuple.FileName, tuple.Content, metadataJson))
          )
      );
  }

  private static JsonArray VariablesToJsonArray(IEnumerable<ProjectEnvironmentVariable> updatedVariables)
  {
    return new JsonArray(
      updatedVariables.Select(variable => new JsonObject(GetProperties(variable)))
        .Cast<JsonNode>()
        .ToArray()
    );
  }

  private static IEnumerable<KeyValuePair<string, JsonNode?>> GetProperties(ProjectEnvironmentVariable variable)
  {
    if (variable.DefaultValue != null)
    {
      yield return new KeyValuePair<string, JsonNode?>("defaultValue", variable.DefaultValue);
    }

    yield return new KeyValuePair<string, JsonNode?>("description", variable.Description);
    yield return new KeyValuePair<string, JsonNode?>("name", variable.Name);
    yield return new KeyValuePair<string, JsonNode?>("required", variable.Required);
    yield return new KeyValuePair<string, JsonNode?>("secret", variable.Secret);
  }

  private static string GetCiEnvVariablesNodeName(JsonNode ciEnvironmentNode)
  {
    var nodeObject = ciEnvironmentNode.AsObject();
    return nodeObject.Select(item => item.Key)
             .FirstOrDefault(key =>
               key.Equals(ExpectedCiEnvVariablesNodeName, StringComparison.InvariantCultureIgnoreCase))
           ?? ExpectedCiEnvVariablesNodeName;
  }

  private static string GetCiEnvNodeName(JsonObject jsonObject)
  {
    return jsonObject.Select(item => item.Key)
             .FirstOrDefault(item => item.Equals(ExpectedCiEnvNodeName, StringComparison.InvariantCultureIgnoreCase))
           ?? ExpectedCiEnvNodeName;
  }
}
