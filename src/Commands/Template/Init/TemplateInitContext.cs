using Cicee.CiEnv;

namespace Cicee.Commands.Template.Init;

public record TemplateInitContext(
  string ProjectRoot,
  bool OverwriteFiles,
  string MetadataFile,
  ProjectMetadata ProjectMetadata);
