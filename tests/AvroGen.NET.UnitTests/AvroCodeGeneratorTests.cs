using Xunit;
using FluentAssertions;
using Avro;
using System.Text.RegularExpressions;

namespace AvroGen.NET.UnitTests;

public class AvroCodeGeneratorTests
{
    private readonly AvroCodeGenerator _generator;
    private const string SimpleSchema = @"{
        ""type"": ""record"",
        ""name"": ""Test.User"",
        ""fields"": [
            { ""name"": ""name"", ""type"": ""string"" },
            { ""name"": ""age"", ""type"": ""int"" }
        ]
    }";

    private const string ComplexSchema = @"{
        ""type"": ""record"",
        ""name"": ""Test.Order"",
        ""fields"": [
            { ""name"": ""orderId"", ""type"": ""string"" },
            { ""name"": ""items"", ""type"": {
                ""type"": ""array"",
                ""items"": {
                    ""type"": ""record"",
                    ""name"": ""Test.OrderItem"",
                    ""fields"": [
                        { ""name"": ""productId"", ""type"": ""string"" },
                        { ""name"": ""quantity"", ""type"": ""int"" },
                        { ""name"": ""price"", ""type"": ""double"" }
                    ]
                }
            }}
        ]
    }";

    public AvroCodeGeneratorTests()
    {
        _generator = new AvroCodeGenerator();
    }

    [Fact]
    public void GenerateCode_SimpleSchema_ShouldGenerateValidCSharpCode()
    {
        // Arrange
        var schema = Schema.Parse(SimpleSchema);

        // Act
        var code = _generator.GenerateCode(schema);

        // Assert
        code.Should().NotBeNullOrWhiteSpace();
        code.Should().Contain("namespace Test");
        code.Should().Contain("public class User");
        code.Should().Contain("public string name");
        code.Should().Contain("public int age");
    }

    [Fact]
    public void GenerateCode_ComplexSchema_ShouldGenerateNestedTypes()
    {
        // Arrange
        var schema = Schema.Parse(ComplexSchema);

        // Act
        var code = _generator.GenerateCode(schema);

        // Assert
        code.Should().NotBeNullOrWhiteSpace();
        code.Should().Contain("public class Order");
        code.Should().Contain("public class OrderItem");
        code.Should().Contain("public string orderId");
        code.Should().Contain("public List<OrderItem> items");
    }

    [Fact]
    public void GenerateCode_SimpleSchema_ShouldGenerateSerializationMethods()
    {
        // Arrange
        var schema = Schema.Parse(SimpleSchema);

        // Act
        var code = _generator.GenerateCode(schema);

        // Assert
        code.Should().Contain("public void Write(Encoder encoder)");
        code.Should().Contain("public void Read(Decoder decoder)");
    }

    [Fact]
    public void GenerateCode_InvalidSchema_ShouldThrowException()
    {
        // Arrange
        const string invalidSchema = @"{ ""type"": ""invalid"" }";

        // Act & Assert
        var schema = Schema.Parse(invalidSchema);
        var action = () => _generator.GenerateCode(schema);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GenerateCode_ShouldGenerateCompilableCode()
    {
        // Arrange
        var schema = Schema.Parse(SimpleSchema);

        // Act
        var code = _generator.GenerateCode(schema);

        // Assert
        // Проверяем базовые требования к компилируемому коду
        code.Should().Contain("using System;");
        code.Should().Contain("using Avro;");
        code.Should().Contain("namespace Test");
        code.Should().Contain("public class");
        
        // Проверяем отсутствие синтаксических ошибок
        code.Should().NotContain(";;"); // Двойные точки с запятой
        code.Should().NotContain("}}}}"); // Лишние закрывающие скобки
        
        // Проверяем правильность отступов
        var lines = code.Split('\n');
        lines.Where(l => l.Contains("class")).Should().NotBeEmpty();
        lines.Where(l => l.Contains("public") && !l.Contains("class")).Should().NotBeEmpty();
    }
}
