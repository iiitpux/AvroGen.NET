using Confluent.SchemaRegistry;
using Moq;
using Xunit;
using System.IO;

namespace AvroGen.NET.UnitTests;

/// <summary>
/// Модульные тесты для класса SchemaGenerator
/// </summary>
public class SchemaGeneratorTests : IDisposable
{
    private readonly Mock<ISchemaRegistryClient> _schemaRegistryMock;
    private const string TestSchema = @"{
        ""type"": ""record"",
        ""name"": ""TestRecord"",
        ""namespace"": ""Test"",
        ""fields"": [
            { ""name"": ""field1"", ""type"": ""string"" },
            { ""name"": ""field2"", ""type"": ""int"" }
        ]
    }";
    private readonly SchemaGeneratorConfig _config;
    private readonly SchemaGenerator _generator;

    public SchemaGeneratorTests()
    {
        _schemaRegistryMock = new Mock<ISchemaRegistryClient>();
        _config = new SchemaGeneratorConfig
        {
            SchemaRegistryUrl = "http://localhost:8081",
            Subject = "test-subject",
            Version = 1,
            OutputDirectory = "TestOutput",
            Namespace = "TestNamespace"
        };
        _generator = new SchemaGenerator(_config, _schemaRegistryMock.Object);
    }

    /// <summary>
    /// Проверяет генерацию кода для конкретной версии схемы
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithSpecificVersion_GeneratesCode()
    {
        // Arrange
        var registeredSchema = new RegisteredSchema("test-subject", 1, 1, TestSchema, SchemaType.Avro, new List<SchemaReference>());
        _schemaRegistryMock
            .Setup(x => x.GetRegisteredSchemaAsync("test-subject", 1))
            .ReturnsAsync(registeredSchema);

        try
        {
            // Act
            await _generator.GenerateAsync();

            // Assert
            Assert.True(Directory.Exists(_config.OutputDirectory));
            var files = Directory.GetFiles(_config.OutputDirectory);
            Assert.Single(files);
            Assert.Contains("TestRecord.cs", files[0]);
        }
        finally
        {
            if (Directory.Exists(_config.OutputDirectory))
            {
                Directory.Delete(_config.OutputDirectory, true);
            }
        }
    }

    /// <summary>
    /// Проверяет генерацию кода для последней версии схемы
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithLatestVersion_GeneratesCode()
    {
        // Arrange
        var config = new SchemaGeneratorConfig
        {
            SchemaRegistryUrl = "http://localhost:8081",
            Subject = "test-subject",
            Version = null, // Использовать последнюю версию
            OutputDirectory = "TestOutput",
            Namespace = "TestNamespace"
        };
        var generator = new SchemaGenerator(config, _schemaRegistryMock.Object);

        var registeredSchema = new RegisteredSchema("test-subject", 1, 1, TestSchema, SchemaType.Avro, new List<SchemaReference>());
        _schemaRegistryMock
            .Setup(x => x.GetLatestSchemaAsync("test-subject"))
            .ReturnsAsync(registeredSchema);

        try
        {
            // Act
            await generator.GenerateAsync();

            // Assert
            Assert.True(Directory.Exists(config.OutputDirectory));
            var files = Directory.GetFiles(config.OutputDirectory);
            Assert.Single(files);
            Assert.Contains("TestRecord.cs", files[0]);
        }
        finally
        {
            if (Directory.Exists(config.OutputDirectory))
            {
                Directory.Delete(config.OutputDirectory, true);
            }
        }
    }

    /// <summary>
    /// Проверяет обработку ошибки отсутствия схемы
    /// </summary>
    [Fact]
    public async Task GenerateAsync_SchemaNotFound_ThrowsException()
    {
        // Arrange
        var config = new SchemaGeneratorConfig
        {
            SchemaRegistryUrl = "http://localhost:8081",
            Subject = "non-existent-subject",
            OutputDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())
        };

        _schemaRegistryMock
            .Setup(x => x.GetLatestSchemaAsync("non-existent-subject"))
            .ThrowsAsync(new SchemaRegistryException("Subject 'non-existent-subject' not found.", System.Net.HttpStatusCode.NotFound, 40401));

        var generator = new SchemaGenerator(config, _schemaRegistryMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<SchemaRegistryException>(() => generator.GenerateAsync());
    }

    /// <summary>
    /// Очистка после тестов
    /// </summary>
    public void Dispose()
    {
        // Удаляем тестовую директорию, если она существует
        if (Directory.Exists(_config.OutputDirectory))
        {
            Directory.Delete(_config.OutputDirectory, true);
        }
    }
}
