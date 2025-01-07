using Xunit;

namespace AvroGen.NET.UnitTests
{
    public class VersionedCodeGenTests
    {
        private readonly string _testOutputPath;

        public VersionedCodeGenTests()
        {
            _testOutputPath = Path.Combine(Path.GetTempPath(), "AvroGenTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testOutputPath);
        }

        [Fact]
        public void WriteTypes_WithVersionedSchema_AddsVersionComment()
        {
            // Arrange
            var codegen = new VersionedCodeGen(2);
            
            var schemaJson = @"{
                ""type"": ""record"",
                ""name"": ""TestRecord"",
                ""namespace"": ""Test.Namespace"",
                ""fields"": [
                    { ""name"": ""testField"", ""type"": ""string"" }
                ]
            }";

            codegen.AddSchema(schemaJson);
            codegen.GenerateCode();

            // Act
            codegen.WriteTypes(_testOutputPath);

            // Assert
            var generatedFile = Path.Combine(_testOutputPath, "Test", "Namespace", "TestRecord.cs");
            Assert.True(File.Exists(generatedFile));
            
            var content = File.ReadAllText(generatedFile);
            Assert.Contains("// Schema version: 2", content);
            Assert.Contains("public partial class TestRecord", content);
        }

        [Fact]
        public void WriteTypes_WithSkipDirectories_GeneratesFilesInRoot()
        {
            // Arrange
            const int schemaVersion = 1;
            var codegen = new VersionedCodeGen(schemaVersion);
            
            var schemaJson = @"{
                ""type"": ""record"",
                ""name"": ""TestRecord"",
                ""namespace"": ""Test.Namespace"",
                ""fields"": [
                    { ""name"": ""testField"", ""type"": ""string"" }
                ]
            }";

            codegen.AddSchema(schemaJson);
            codegen.GenerateCode();

            // Act
            codegen.WriteTypes(_testOutputPath, true);

            // Assert
            var generatedFileInRoot = Path.Combine(_testOutputPath, "TestRecord.cs");
            var generatedFileInNamespace = Path.Combine(_testOutputPath, "Test.Namespace", "TestRecord.cs");
            
            Assert.True(File.Exists(generatedFileInRoot));
            Assert.False(Directory.Exists(Path.Combine(_testOutputPath, "Test.Namespace")));
            Assert.False(File.Exists(generatedFileInNamespace));
            
            var content = File.ReadAllText(generatedFileInRoot);
            Assert.Contains("// Schema version: 1", content);
        }

        [Fact]
        public void WriteTypes_GeneratesSchemaVersionComment()
        {
            // Arrange
            var codegen = new VersionedCodeGen(1);
            var schemaJson = @"{
                ""type"": ""record"",
                ""name"": ""TestRecord"",
                ""namespace"": ""Test.Namespace"",
                ""fields"": [
                    { ""name"": ""testField"", ""type"": ""string"" }
                ]
            }";

            codegen.AddSchema(schemaJson);
            codegen.GenerateCode();

            // Act
            codegen.WriteTypes(_testOutputPath);

            // Assert
            var generatedFile = Path.Combine(_testOutputPath, "Test", "Namespace", "TestRecord.cs");
            Assert.True(File.Exists(generatedFile));
            
            var content = File.ReadAllText(generatedFile);
            Assert.Contains("// Schema version: 1", content);
            Assert.Contains("public partial class TestRecord", content);
        }

        [Fact]
        public void WriteTypes_WithFixedType_GeneratesCorrectCode()
        {
            // Arrange
            var codegen = new VersionedCodeGen(3);
            var schemaJson = @"{
                ""type"": ""fixed"",
                ""name"": ""TestFixed"",
                ""namespace"": ""Test.Namespace"",
                ""size"": 16
            }";

            codegen.AddSchema(schemaJson);
            codegen.GenerateCode();

            // Act
            codegen.WriteTypes(_testOutputPath);

            // Assert
            var generatedFile = Path.Combine(_testOutputPath, "Test", "Namespace", "TestFixed.cs");
            Assert.True(File.Exists(generatedFile));
            
            var content = File.ReadAllText(generatedFile);
            Assert.Contains("// Schema version: 3", content);
            Assert.Contains("public partial class TestFixed : SpecificFixed", content);
            Assert.Contains("private static uint fixedSize = 16;", content);
        }
    }
}
