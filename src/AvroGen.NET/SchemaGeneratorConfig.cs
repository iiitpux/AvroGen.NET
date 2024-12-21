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
        /// Gets or sets the output directory for generated classes.
        /// </summary>
        public required string OutputDirectory { get; set; }
    }
}
