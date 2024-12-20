using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using System.Text.Json;
using com.example;

// Configure Schema Registry client
var schemaRegistryUrl = "http://localhost:8081";
var schemaRegistryConfig = new SchemaRegistryConfig { Url = schemaRegistryUrl };
var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);

// Wait for schema to be generated
Console.WriteLine("Waiting for schema to be generated...");
await Task.Delay(2000); // Give MSBuild time to generate the classes

try
{
    // Create an instance of the generated class
    var testRecord = new TestRecord
    {
        StringField = "Hello, Avro!",
        IntField = 42,
        BooleanField = true,
        DoubleField = 2.71828,
        ArrayField = new[] { "one", "two", "three" }.ToList(),
        EnumField = TestEnum.ONE
    };

    Console.WriteLine("Created test record:");
    Console.WriteLine(JsonSerializer.Serialize(testRecord, new JsonSerializerOptions { WriteIndented = true }));

    // Configure Avro serializer
    var serializerConfig = new AvroSerializerConfig
    {
        AutoRegisterSchemas = true
    };

    // Create serializer and deserializer
    var serializer = new AvroSerializer<TestRecord>(schemaRegistry, serializerConfig);
    var deserializer = new AvroDeserializer<TestRecord>(schemaRegistry);

    // Serialize
    Console.WriteLine("\nSerializing record...");
    var serialized = await serializer.SerializeAsync(testRecord, 
        new Confluent.Kafka.SerializationContext(Confluent.Kafka.MessageComponentType.Value, "test-topic"));
    Console.WriteLine($"Serialized size: {serialized.Length} bytes");

    // Deserialize
    Console.WriteLine("\nDeserializing record...");
    var deserialized = await deserializer.DeserializeAsync(serialized, false, 
        new Confluent.Kafka.SerializationContext(Confluent.Kafka.MessageComponentType.Value, "test-topic"));
    
    Console.WriteLine("Deserialized record:");
    Console.WriteLine(JsonSerializer.Serialize(deserialized, new JsonSerializerOptions { WriteIndented = true }));

    // Verify
    Console.WriteLine("\nVerifying serialization/deserialization...");
    var areEqual = JsonSerializer.Serialize(testRecord) == JsonSerializer.Serialize(deserialized);
    Console.WriteLine($"Original and deserialized records are {(areEqual ? "equal" : "not equal")}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
