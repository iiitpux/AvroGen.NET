using Xunit;
using System.IO;
using Confluent.SchemaRegistry;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AvroGen.NET.UnitTests
{
    public class SchemaGeneratorTests
    {
        private readonly string _testOutputPath;
        private readonly Mock<ISchemaRegistryClient> _mockSchemaRegistry;

        public SchemaGeneratorTests()
        {
            _testOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "Generated");
            _mockSchemaRegistry = new Mock<ISchemaRegistryClient>();
        }

        [Fact]
        public async Task GenerateAsync_WithValidSchema_GeneratesClassWithVersionComment()
        {
            // Arrange
            const int schemaVersion = 1;
            var schemaJson = @"{
                ""type"": ""record"",
                ""name"": ""TestRecord"",
                ""namespace"": ""Test.Namespace"",
                ""fields"": [
                    { ""name"": ""testField"", ""type"": ""string"" }
                ]
            }";

            _mockSchemaRegistry
                .Setup(x => x.GetRegisteredSchemaAsync("test-subject", schemaVersion))
                .ReturnsAsync(new RegisteredSchema("test-subject", schemaVersion, 1, schemaJson, SchemaType.Avro, new List<SchemaReference>()));

            var config = new SchemaGeneratorConfig
            {
                SchemaRegistryUrl = "http://localhost:8081",
                Subject = "test-subject",
                Version = schemaVersion,
                OutputDirectory = _testOutputPath,
                CreateDirectoryStructure = false
            };

            var generator = new SchemaGenerator(config, _mockSchemaRegistry.Object);

            // Act
            await generator.GenerateAsync();

            // Assert
            var generatedFile = Path.Combine(_testOutputPath, "TestRecord.cs");
            Assert.True(File.Exists(generatedFile));
            
            var content = File.ReadAllText(generatedFile);
            Assert.Contains("// Schema version: 1", content);
            Assert.Contains("public partial class TestRecord", content);
        }

        [Fact]
        public async Task GenerateAsync_WithNamespace_GeneratesClassInNamespace()
        {
            // Arrange
            const int schemaVersion = 2;
            var schemaJson = @"{
                ""type"": ""record"",
                ""name"": ""TestRecord"",
                ""namespace"": ""Test.Namespace"",
                ""fields"": [
                    { ""name"": ""testField"", ""type"": ""string"" }
                ]
            }";

            _mockSchemaRegistry
                .Setup(x => x.GetRegisteredSchemaAsync("test-subject", schemaVersion))
                .ReturnsAsync(new RegisteredSchema("test-subject", schemaVersion, 1, schemaJson, SchemaType.Avro, new List<SchemaReference>()));

            var config = new SchemaGeneratorConfig
            {
                SchemaRegistryUrl = "http://localhost:8081",
                Subject = "test-subject",
                Version = schemaVersion,
                OutputDirectory = _testOutputPath,
                Namespace = "Custom.Namespace",
                CreateDirectoryStructure = true
            };

            var generator = new SchemaGenerator(config, _mockSchemaRegistry.Object);

            // Act
            await generator.GenerateAsync();

            // Assert
            var generatedFile = Path.Combine(_testOutputPath, "Custom", "Namespace", "TestRecord.cs");
            Assert.True(File.Exists(generatedFile));
            
            var content = File.ReadAllText(generatedFile);
            Assert.Contains("namespace Custom.Namespace", content);
            Assert.Contains("// Schema version: 2", content);
            Assert.Contains("public partial class TestRecord", content);
        }

        [Fact]
        public async Task GenerateAsync_WithLatestVersion_UsesLatestSchema()
        {
            // Arrange
            const int latestVersion = 3;
            var schemaJson = @"{
                ""type"": ""record"",
                ""name"": ""TestRecord"",
                ""namespace"": ""Test.Namespace"",
                ""fields"": [
                    { ""name"": ""testField"", ""type"": ""string"" }
                ]
            }";

            _mockSchemaRegistry
                .Setup(x => x.GetLatestSchemaAsync("test-subject"))
                .ReturnsAsync(new RegisteredSchema("test-subject", latestVersion, 1, schemaJson, SchemaType.Avro, new List<SchemaReference>()));

            var config = new SchemaGeneratorConfig
            {
                SchemaRegistryUrl = "http://localhost:8081",
                Subject = "test-subject",
                OutputDirectory = _testOutputPath,
                CreateDirectoryStructure = false
            };

            var generator = new SchemaGenerator(config, _mockSchemaRegistry.Object);

            // Act
            await generator.GenerateAsync();

            // Assert
            var generatedFile = Path.Combine(_testOutputPath, "TestRecord.cs");
            Assert.True(File.Exists(generatedFile));
            
            var content = File.ReadAllText(generatedFile);
            Assert.Contains("// Schema version: 3", content);
            Assert.Contains("public partial class TestRecord", content);
        }
    }
}
