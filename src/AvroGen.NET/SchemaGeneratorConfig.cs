namespace AvroGen.NET
{
    /// <summary>
    /// Configuration for the schema generator.
    /// </summary>
    public class SchemaGeneratorConfig
    {
        /// <summary>
        /// Gets or sets the URL of the Schema Registry.
        /// </summary>
        public required string SchemaRegistryUrl { get; set; }

        /// <summary>
        /// Gets or sets the schema subject name in Schema Registry.
        /// </summary>
        public required string Subject { get; set; }

        /// <summary>
        /// Gets or sets the schema version in Schema Registry. If not specified, latest version will be used.
        /// </summary>
        public int? Version { get; set; }

        /// <summary>
        /// Gets or sets the output directory for generated classes.
        /// </summary>
        public required string OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the namespace for generated classes.
        /// </summary>
        public string? Namespace { get; set; }

        /// <summary>
        /// Создавать ли структуру директорий по пространствам имен.
        /// </summary>
        public bool CreateDirectoryStructure { get; set; } = true;
    }
}
