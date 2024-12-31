using Avro;
using Xunit;

namespace AvroGen.NET.UnitTests;

/// <summary>
/// Модульные тесты для класса AvroCodeGenerator
/// </summary>
public class AvroCodeGeneratorTests
{
    private readonly AvroCodeGenerator _generator;

    public AvroCodeGeneratorTests()
    {
        _generator = new AvroCodeGenerator();
    }

    /// <summary>
    /// Проверяет генерацию кода для простой схемы
    /// </summary>
    [Fact]
    public void GenerateCode_SimpleSchema_GeneratesValidClass()
    {
        // Arrange
        var schema = Schema.Parse(@"{
            ""type"": ""record"",
            ""name"": ""TestRecord"",
            ""namespace"": ""TestNamespace"",
            ""fields"": [
                { ""name"": ""testField"", ""type"": ""string"" }
            ]
        }");

        // Act
        var code = _generator.GenerateCode(schema);

        // Assert
        Assert.Contains("namespace TestNamespace", code);
        Assert.Contains("public class TestRecord", code);
        Assert.Contains("public string testField", code);
    }

    /// <summary>
    /// Проверяет генерацию кода для схемы с вложенными типами
    /// </summary>
    [Fact]
    public void GenerateCode_NestedSchema_GeneratesValidClass()
    {
        // Arrange
        var schema = Schema.Parse(@"{
            ""type"": ""record"",
            ""name"": ""ParentRecord"",
            ""namespace"": ""TestNamespace"",
            ""fields"": [
                {
                    ""name"": ""child"",
                    ""type"": {
                        ""type"": ""record"",
                        ""name"": ""ChildRecord"",
                        ""fields"": [
                            { ""name"": ""childField"", ""type"": ""string"" }
                        ]
                    }
                }
            ]
        }");

        // Act
        var code = _generator.GenerateCode(schema);

        // Assert
        Assert.Contains("namespace TestNamespace", code);
        Assert.Contains("public class ParentRecord", code);
        Assert.Contains("public class ChildRecord", code);
        Assert.Contains("public ChildRecord child", code);
        Assert.Contains("public string childField", code);
    }

    /// <summary>
    /// Проверяет генерацию кода для схемы с массивом
    /// </summary>
    [Fact]
    public void GenerateCode_ArraySchema_GeneratesValidClass()
    {
        // Arrange
        var schema = Schema.Parse(@"{
            ""type"": ""record"",
            ""name"": ""ArrayRecord"",
            ""namespace"": ""TestNamespace"",
            ""fields"": [
                {
                    ""name"": ""items"",
                    ""type"": {
                        ""type"": ""array"",
                        ""items"": ""string""
                    }
                }
            ]
        }");

        // Act
        var code = _generator.GenerateCode(schema);

        // Assert
        Assert.Contains("namespace TestNamespace", code);
        Assert.Contains("public class ArrayRecord", code);
        Assert.Contains("public System.Collections.Generic.List<string> items", code);
    }

    /// <summary>
    /// Проверяет обработку null схемы
    /// </summary>
    [Fact]
    public void GenerateCode_NullSchema_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _generator.GenerateCode(null));
    }

    /// <summary>
    /// Проверяет обработку невалидной схемы
    /// </summary>
    [Fact]
    public void GenerateCode_NonRecordSchema_ThrowsArgumentException()
    {
        // Arrange
        var schema = Schema.Parse(@"{ ""type"": ""string"" }");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _generator.GenerateCode(schema));
    }
}
