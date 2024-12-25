using Xunit;
using FluentAssertions;
using Confluent.SchemaRegistry;
using Moq;
using Avro;
using System.Text;

namespace AvroGen.NET.IntegrationTests;

public class SchemaRegistryIntegrationTests
{
    private readonly Mock<ISchemaRegistryClient> _mockSchemaRegistry;
    private readonly AvroCodeGenerator _generator;
    private const string TestSchema = @"{
        ""type"": ""record"",
        ""name"": ""User"",
        ""namespace"": ""Test"",
        ""fields"": [
            { ""name"": ""name"", ""type"": ""string"" },
            { ""name"": ""age"", ""type"": ""int"" }
        ]
    }";

    public SchemaRegistryIntegrationTests()
    {
        _mockSchemaRegistry = new Mock<ISchemaRegistryClient>();
        _generator = new AvroCodeGenerator();

        // Настраиваем мок для возврата тестовой схемы
        _mockSchemaRegistry
            .Setup(x => x.GetLatestSchemaAsync("test-subject"))
            .ReturnsAsync(new RegisteredSchema("test-subject", 1, 1, TestSchema));
    }

    [Fact]
    public async Task GetSchema_ShouldReturnValidSchema()
    {
        // Act
        var schema = await _mockSchemaRegistry.Object.GetLatestSchemaAsync("test-subject");

        // Assert
        schema.Should().NotBeNull();
        schema.SchemaString.Should().Be(TestSchema);
        schema.Subject.Should().Be("test-subject");
    }

    [Fact]
    public async Task GenerateCode_FromSchemaRegistry_ShouldProduceValidCode()
    {
        // Arrange
        var registeredSchema = await _mockSchemaRegistry.Object.GetLatestSchemaAsync("test-subject");
        var avroSchema = Schema.Parse(registeredSchema.SchemaString);

        // Act
        var code = _generator.GenerateCode(avroSchema);

        // Assert
        code.Should().NotBeNullOrWhiteSpace();
        code.Should().Contain("namespace Test");
        code.Should().Contain("public class User");
        code.Should().Contain("public string name");
        code.Should().Contain("public int age");
    }

    [Fact]
    public async Task GenerateCode_FromSchemaRegistry_ShouldCompile()
    {
        // Arrange
        var registeredSchema = await _mockSchemaRegistry.Object.GetLatestSchemaAsync("test-subject");
        var avroSchema = Schema.Parse(registeredSchema.SchemaString);

        // Act
        var code = _generator.GenerateCode(avroSchema);

        // Assert
        var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(code);
        var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
            "DynamicAssembly",
            new[] { syntaxTree },
            new[] {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Avro.Schema).Assembly.Location)
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var result = compilation.Emit(new MemoryStream());
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetSchema_NonExistentSubject_ShouldThrowException()
    {
        // Arrange
        _mockSchemaRegistry
            .Setup(x => x.GetLatestSchemaAsync("non-existent"))
            .ThrowsAsync(new SchemaRegistryException("Subject not found"));

        // Act & Assert
        await _mockSchemaRegistry.Object
            .Invoking(x => x.GetLatestSchemaAsync("non-existent"))
            .Should().ThrowAsync<SchemaRegistryException>()
            .WithMessage("Subject not found");
    }
}
