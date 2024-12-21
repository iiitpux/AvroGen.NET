using Microsoft.Build.Framework;
using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace AvroGen.NET
{
    public class GenerateAvroClassesTask : MSBuildTask
    {
        [Required]
        public ITaskItem[] Schemas { get; set; } = Array.Empty<ITaskItem>();

        [Required]
        public string SchemaRegistryUrl { get; set; } = string.Empty;

        [Required]
        public string OutputDirectory { get; set; } = string.Empty;

        public override bool Execute()
        {
            try
            {
                Log.LogMessage(MessageImportance.High, "Starting Avro class generation...");
                Log.LogMessage(MessageImportance.High, $"MSBuildProjectDirectory: {BuildEngine.ProjectFileOfTaskNode}");

                var config = new SchemaGeneratorConfig
                {
                    SchemaRegistryUrl = SchemaRegistryUrl,
                    OutputDirectory = OutputDirectory
                };

                foreach (var schema in Schemas)
                {
                    var subject = schema.GetMetadata("Subject");
                    var version = int.Parse(schema.GetMetadata("Version"));
                    var outputPath = schema.GetMetadata("OutputPath");

                    Log.LogMessage(MessageImportance.High, $"Processing schema: {subject} version {version}");
                    Log.LogMessage(MessageImportance.High, $"Output Path: {outputPath}");

                    var generator = new SchemaGenerator(config);
                    generator.GenerateClassFromSchema(subject, version).Wait();

                    Log.LogMessage(MessageImportance.High, 
                        $"Generated classes for schema {subject} version {version}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }
        }
    }
}
