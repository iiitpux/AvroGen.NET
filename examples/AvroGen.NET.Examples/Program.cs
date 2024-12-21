using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;

namespace AvroGen.NET.Examples;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Example configuration
        var schemaRegistryConfig = new SchemaRegistryConfig
        {
            // Schema Registry REST endpoint
            Url = "http://localhost:8081"
        };

        // Create Schema Registry client
        using var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);

        // Example usage of AvroGen.NET
        Console.WriteLine("Example of using AvroGen.NET");
        
        // Add your example code here
    }
}
