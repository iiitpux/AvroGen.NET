using Microsoft.Build.Framework;
using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace AvroGen.NET
{
    public class GenerateAvroClassesTask : MSBuildTask
    {
        [Required]
        public ITaskItem[] AvroGen { get; set; } = Array.Empty<ITaskItem>();

        public override bool Execute()
        {
            try
            {
                Log.LogMessage(MessageImportance.High, "Starting Avro class generation...");
                Log.LogMessage(MessageImportance.High, $"MSBuildProjectDirectory: {BuildEngine.ProjectFileOfTaskNode}");

                foreach (var schema in AvroGen)
                {
                    var subject = schema.GetMetadata("Subject");
                    var version = int.Parse(schema.GetMetadata("Version"));
                    var schemaRegistryUrl = schema.GetMetadata("SchemaRegistryUrl");
                    var outputPath = schema.GetMetadata("OutputPath");

                    Log.LogMessage(MessageImportance.High, $"Processing schema: {subject} version {version}");
                    Log.LogMessage(MessageImportance.High, $"Schema Registry URL: {schemaRegistryUrl}");
                    Log.LogMessage(MessageImportance.High, $"Output Path: {outputPath}");

                    var config = new SchemaGeneratorConfig
                    {
                        SchemaRegistryUrl = schemaRegistryUrl,
                        OutputDirectory = outputPath
                    };

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
