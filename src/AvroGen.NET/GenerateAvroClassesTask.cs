using Microsoft.Build.Framework;
using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace AvroGen.NET
{
    /// <summary>
    /// MSBuild task for generating C# classes from Avro schemas in Schema Registry.
    /// </summary>
    public class GenerateAvroClassesTask : MSBuildTask
    {
        /// <summary>
        /// Gets or sets the schema items to process.
        /// Each item must have the following metadata:
        /// - SchemaRegistryUrl: URL of the Schema Registry
        /// - Subject: Schema subject in Schema Registry
        /// - Version: Schema version in Schema Registry (optional)
        /// - OutputPath: Output directory for generated classes
        /// - Namespace: Namespace for generated classes (optional)
        /// </summary>
        [Required]
        public ITaskItem[] Schemas { get; set; } = Array.Empty<ITaskItem>();

        /// <summary>
        /// Executes the task to generate C# classes from Avro schemas.
        /// </summary>
        /// <returns>True if task execution was successful; otherwise, false.</returns>
        public override bool Execute()
        {
            //todo- delete this
            //System.Diagnostics.Debugger.Launch();
            try
            {
                Log.LogMessage(MessageImportance.High, "Starting Avro class generation...");

                foreach (var schema in Schemas)
                {
                    var config = new SchemaGeneratorConfig
                    {
                        SchemaRegistryUrl = schema.GetMetadata("SchemaRegistryUrl"),
                        Subject = schema.GetMetadata("Subject"),
                        OutputDirectory = schema.GetMetadata("OutputDirectory"),
                        Namespace = schema.GetMetadata("Namespace")
                    };

                    if (int.TryParse(schema.GetMetadata("Version"), out var version))
                    {
                        config.Version = version;
                    }

                    // Log configuration details
                    LogConfiguration(schema, config);

                    // Create output directory
                    Directory.CreateDirectory(config.OutputDirectory);

                    // Generate classes
                    var generator = new SchemaGenerator(config);
                    generator.GenerateAsync().Wait();
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex, true);
                return false;
            }
        }

        private void LogConfiguration(ITaskItem schema, SchemaGeneratorConfig config)
        {
            Log.LogMessage(MessageImportance.High, $"Processing schema item: {schema.ItemSpec}");
            Log.LogMessage(MessageImportance.High, $"Schema Registry URL: {config.SchemaRegistryUrl}");
            Log.LogMessage(MessageImportance.High, $"Subject: {config.Subject}");
            Log.LogMessage(MessageImportance.High, $"Version: {config.Version?.ToString() ?? "latest"}");
            Log.LogMessage(MessageImportance.High, $"Output Directory: {config.OutputDirectory}");
            
            if (!string.IsNullOrEmpty(config.Namespace))
            {
                Log.LogMessage(MessageImportance.High, $"Namespace: {config.Namespace}");
            }
        }
    }
}
