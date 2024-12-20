using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using System.Text.Json;
using com.example;

const string bootstrapServers = "localhost:29092";
const string topic = "test-topic";
const string groupId = "test-consumer-group";

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

    // Configure Kafka producer
    var producerConfig = new ProducerConfig
    {
        BootstrapServers = bootstrapServers
    };

    // Configure Avro serializer
    var serializerConfig = new AvroSerializerConfig
    {
        AutoRegisterSchemas = true
    };

    // Create serializer and deserializer
    var serializer = new AvroSerializer<TestRecord>(schemaRegistry, serializerConfig);
    var deserializer = new AvroDeserializer<TestRecord>(schemaRegistry);

    // Create producer
    using var producer = new ProducerBuilder<string, byte[]>(producerConfig).Build();

    // Serialize and produce message
    Console.WriteLine($"\nProducing message to topic {topic}...");
    var serialized = await serializer.SerializeAsync(testRecord, new SerializationContext(MessageComponentType.Value, topic));
    var deliveryResult = await producer.ProduceAsync(topic, new Message<string, byte[]>
    {
        Key = "test-key",
        Value = serialized
    });
    Console.WriteLine($"Message delivered to: {deliveryResult.TopicPartitionOffset}");

    // Configure Kafka consumer
    var consumerConfig = new ConsumerConfig
    {
        BootstrapServers = bootstrapServers,
        GroupId = groupId,
        AutoOffsetReset = AutoOffsetReset.Earliest
    };

    // Create consumer
    using var consumer = new ConsumerBuilder<string, byte[]>(consumerConfig).Build();

    // Subscribe to topic
    consumer.Subscribe(topic);

    Console.WriteLine($"\nConsuming message from topic {topic}...");
    
    // Consume message with timeout
    var consumeResult = consumer.Consume(TimeSpan.FromSeconds(10));
    
    if (consumeResult != null)
    {
        Console.WriteLine("Received message:");
        Console.WriteLine($"Key: {consumeResult.Message.Key}");
        
        // Deserialize message
        var deserialized = await deserializer.DeserializeAsync(consumeResult.Message.Value, false, 
            new SerializationContext(MessageComponentType.Value, topic));
        
        Console.WriteLine("Value:");
        Console.WriteLine(JsonSerializer.Serialize(deserialized, 
            new JsonSerializerOptions { WriteIndented = true }));

        // Verify
        Console.WriteLine("\nVerifying producer/consumer...");
        var areEqual = JsonSerializer.Serialize(testRecord) == 
            JsonSerializer.Serialize(deserialized);
        Console.WriteLine($"Original and consumed records are {(areEqual ? "equal" : "not equal")}");
    }
    else
    {
        Console.WriteLine("No message received within timeout");
    }

    // Clean up
    consumer.Close();
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
