using Avro;
using Xunit;

namespace AvroGen.NET.UnitTests;

/// <summary>
/// Unit tests for AvroCodeGenerator class
/// </summary>
public class AvroCodeGeneratorTests
{
    private readonly AvroCodeGenerator _generator;

    public AvroCodeGeneratorTests()
    {
        _generator = new AvroCodeGenerator();
    }

    /// <summary>
    /// Tests code generation for a simple schema
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
        var result = _generator.GenerateCode(schema);

        // Assert
        Assert.True(result.ContainsKey("TestRecord.cs"));
        var code = result["TestRecord.cs"];
        Assert.Contains("namespace TestNamespace", code);
        Assert.Contains("public class TestRecord", code);
        Assert.Contains("public string testField", code);
        Assert.Contains("ISpecificRecord", code);
    }

    /// <summary>
    /// Tests code generation for a schema with nested types
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
        var result = _generator.GenerateCode(schema);

        // Assert
        Assert.True(result.ContainsKey("ParentRecord.cs"));
        Assert.True(result.ContainsKey("ChildRecord.cs"));
        
        var parentCode = result["ParentRecord.cs"];
        Assert.Contains("namespace TestNamespace", parentCode);
        Assert.Contains("public class ParentRecord", parentCode);
        Assert.Contains("public ChildRecord child", parentCode);
        Assert.Contains("ISpecificRecord", parentCode);

        var childCode = result["ChildRecord.cs"];
        Assert.Contains("namespace TestNamespace", childCode);
        Assert.Contains("public class ChildRecord", childCode);
        Assert.Contains("public string childField", childCode);
        Assert.Contains("ISpecificRecord", childCode);
    }

    /// <summary>
    /// Tests code generation for a schema with array type
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
        var result = _generator.GenerateCode(schema);

        // Assert
        Assert.True(result.ContainsKey("ArrayRecord.cs"));
        var code = result["ArrayRecord.cs"];
        Assert.Contains("namespace TestNamespace", code);
        Assert.Contains("public class ArrayRecord", code);
        Assert.Contains("public System.Collections.Generic.List<string> items", code);
        Assert.Contains("ISpecificRecord", code);
    }

    /// <summary>
    /// Tests code generation for a schema with enumeration type
    /// </summary>
    [Fact]
    public void GenerateCode_EnumSchema_GeneratesValidClass()
    {
        // Arrange
        var schema = Schema.Parse(@"{
            ""type"": ""record"",
            ""name"": ""EnumRecord"",
            ""namespace"": ""TestNamespace"",
            ""fields"": [
                {
                    ""name"": ""enumField"",
                    ""type"": {
                        ""type"": ""enum"",
                        ""name"": ""TestEnum"",
                        ""symbols"": [""ONE"", ""TWO"", ""THREE""]
                    }
                }
            ]
        }");

        // Act
        var result = _generator.GenerateCode(schema);

        // Assert
        Assert.True(result.ContainsKey("EnumRecord.cs"));
        Assert.True(result.ContainsKey("TestEnum.cs"));

        var enumCode = result["TestEnum.cs"];
        Assert.Contains("public enum TestEnum", enumCode);
        Assert.Contains("ONE", enumCode);
        Assert.Contains("TWO", enumCode);
        Assert.Contains("THREE", enumCode);

        var recordCode = result["EnumRecord.cs"];
        Assert.Contains("public TestEnum enumField", recordCode);
        Assert.Contains("ISpecificRecord", recordCode);
    }

    /// <summary>
    /// Tests code generation for a schema with fixed type
    /// </summary>
    [Fact]
    public void GenerateCode_FixedSchema_GeneratesValidClass()
    {
        // Arrange
        var schema = Schema.Parse(@"{
            ""type"": ""record"",
            ""name"": ""FixedRecord"",
            ""namespace"": ""TestNamespace"",
            ""fields"": [
                {
                    ""name"": ""fixedField"",
                    ""type"": {
                        ""type"": ""fixed"",
                        ""name"": ""TestFixed"",
                        ""size"": 16
                    }
                }
            ]
        }");

        // Act
        var result = _generator.GenerateCode(schema);

        // Assert
        Assert.True(result.ContainsKey("FixedRecord.cs"));
        Assert.True(result.ContainsKey("TestFixed.cs"));

        var fixedCode = result["TestFixed.cs"];
        Assert.Contains("public class TestFixed", fixedCode);
        Assert.Contains("public const int SIZE = 16", fixedCode);
        Assert.Contains("public byte[] Value", fixedCode);
        Assert.Contains("StructLayoutAttribute", fixedCode);

        var recordCode = result["FixedRecord.cs"];
        Assert.Contains("public TestFixed fixedField", recordCode);
        Assert.Contains("ISpecificRecord", recordCode);
    }

    /// <summary>
    /// Tests code generation for a schema with logical types
    /// </summary>
    [Fact]
    public void GenerateCode_LogicalTypeSchema_GeneratesValidClass()
    {
        // Arrange
        var schema = Schema.Parse(@"{
            ""type"": ""record"",
            ""name"": ""LogicalRecord"",
            ""namespace"": ""TestNamespace"",
            ""fields"": [
                {
                    ""name"": ""timestampField"",
                    ""type"": {
                        ""type"": ""long"",
                        ""logicalType"": ""timestamp-millis""
                    }
                },
                {
                    ""name"": ""dateField"",
                    ""type"": {
                        ""type"": ""int"",
                        ""logicalType"": ""date""
                    }
                },
                {
                    ""name"": ""decimalField"",
                    ""type"": {
                        ""type"": ""bytes"",
                        ""logicalType"": ""decimal"",
                        ""precision"": 9,
                        ""scale"": 2
                    }
                }
            ]
        }");

        // Act
        var result = _generator.GenerateCode(schema);

        // Assert
        Assert.True(result.ContainsKey("LogicalRecord.cs"));
        var code = result["LogicalRecord.cs"];
        Assert.Contains("public System.DateTime timestampField", code);
        Assert.Contains("public System.DateTime dateField", code);
        Assert.Contains("public decimal decimalField", code);
        Assert.Contains("ISpecificRecord", code);
    }

    /// <summary>
    /// Tests handling of null schema
    /// </summary>
    [Fact]
    public void GenerateCode_NullSchema_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _generator.GenerateCode(null));
    }

    /// <summary>
    /// Tests handling of invalid schema type
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
