using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using System.Text.Json;

var schemaRegistryUrl = "http://localhost:8081";
var schemaRegistryConfig = new SchemaRegistryConfig { Url = schemaRegistryUrl };
var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);

// Wait for schema to be generated
Console.WriteLine("Waiting for schema to be generated...");
await Task.Delay(2000); // Give MSBuild time to generate the classes

// Try to use the generated class
try
{
    // The actual type will be available after the schema is generated and compiled
    Console.WriteLine("Generated classes should be available in the Generated folder");
    Console.WriteLine("Check the Generated folder for the output files");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
