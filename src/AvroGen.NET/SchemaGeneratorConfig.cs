namespace AvroGen.NET
{
    public class SchemaGeneratorConfig
    {
        public required string SchemaRegistryUrl { get; set; }
        public required string OutputDirectory { get; set; }
    }
}
