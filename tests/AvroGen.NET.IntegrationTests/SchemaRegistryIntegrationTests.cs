using Xunit;
using FluentAssertions;
using Confluent.SchemaRegistry;

namespace AvroGen.NET.IntegrationTests;

public class SchemaRegistryIntegrationTests
{
    private readonly SchemaRegistryConfig _config;
    
    public SchemaRegistryIntegrationTests()
    {
        _config = new SchemaRegistryConfig
        {
            Url = "http://localhost:8081"
        };
    }
    
    [Fact]
    public async Task GetSchema_ShouldReturnValidSchema()
    {
        // Arrange
        using var schemaRegistry = new CachedSchemaRegistryClient(_config);
        
        // Act
        
        // Assert
    }
}
