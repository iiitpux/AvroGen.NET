using AvroGen.NET.Test.Infrastructure;
using Confluent.SchemaRegistry.Serdes;
using FluentAssertions;
using Xunit;
using com.example;

namespace AvroGen.NET.Test;

public class GeneratedClassTests : SchemaRegistryTestBase
{
    [Fact]
    public void Generated_Class_Should_Have_All_Fields()
    {
        // Arrange & Act
        var record = new TestRecord
        {
            StringField = "test",
            IntField = 42,
            BooleanField = true,
            DoubleField = 2.71828,
            ArrayField = new[] { "one", "two", "three" }.ToList(),
            EnumField = TestEnum.ONE
        };

        // Assert
        record.Should().NotBeNull();
        record.StringField.Should().Be("test");
        record.IntField.Should().Be(42);
        record.BooleanField.Should().BeTrue();
        record.DoubleField.Should().BeApproximately(2.71828, 0.00001);
        record.ArrayField.Should().BeEquivalentTo(new[] { "one", "two", "three" });
        record.EnumField.Should().Be(TestEnum.ONE);
    }

    [Fact]
    public async Task Should_Serialize_And_Deserialize_Generated_Class()
    {
        // Arrange
        var serializerConfig = new AvroSerializerConfig
        {
            AutoRegisterSchemas = true
        };

        var deserializerConfig = new AvroDeserializerConfig();

        var serializer = new AvroSerializer<TestRecord>(SchemaRegistry, serializerConfig);
        var deserializer = new AvroDeserializer<TestRecord>(SchemaRegistry, deserializerConfig);

        var original = new TestRecord
        {
            StringField = "test",
            IntField = 42,
            BooleanField = true,
            DoubleField = 2.71828,
            ArrayField = new[] { "one", "two", "three" }.ToList(),
            EnumField = TestEnum.ONE
        };

        // Act
        var serialized = await serializer.SerializeAsync(original, new Confluent.Kafka.SerializationContext(Confluent.Kafka.MessageComponentType.Value, "test-topic"));
        var deserialized = await deserializer.DeserializeAsync(serialized, false, new Confluent.Kafka.SerializationContext(Confluent.Kafka.MessageComponentType.Value, "test-topic"));

        // Assert
        deserialized.Should().BeEquivalentTo(original, options => options.ComparingByMembers<TestRecord>());
    }
}
