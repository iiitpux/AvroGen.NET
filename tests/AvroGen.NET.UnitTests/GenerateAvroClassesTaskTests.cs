using Xunit;
using FluentAssertions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Moq;
using System.Collections;

namespace AvroGen.NET.UnitTests;

public class GenerateAvroClassesTaskTests
{
    private readonly Mock<IBuildEngine> _mockBuildEngine;
    private readonly GenerateAvroClassesTask _task;
    private readonly List<BuildErrorEventArgs> _errors;
    private readonly List<BuildMessageEventArgs> _messages;

    public GenerateAvroClassesTaskTests()
    {
        _mockBuildEngine = new Mock<IBuildEngine>();
        _errors = new List<BuildErrorEventArgs>();
        _messages = new List<BuildMessageEventArgs>();

        _mockBuildEngine
            .Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>()))
            .Callback<BuildErrorEventArgs>(e => _errors.Add(e));

        _mockBuildEngine
            .Setup(x => x.LogMessageEvent(It.IsAny<BuildMessageEventArgs>()))
            .Callback<BuildMessageEventArgs>(m => _messages.Add(m));

        _task = new GenerateAvroClassesTask
        {
            BuildEngine = _mockBuildEngine.Object
        };
    }

    [Fact]
    public void Execute_WithValidSchema_ShouldGenerateCode()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var schemaFile = Path.Combine(tempDir, "test.avsc");
        var outputFile = Path.Combine(tempDir, "Test.User.cs");

        File.WriteAllText(schemaFile, @"{
            ""type"": ""record"",
            ""name"": ""User"",
            ""namespace"": ""Test"",
            ""fields"": [
                { ""name"": ""name"", ""type"": ""string"" },
                { ""name"": ""age"", ""type"": ""int"" }
            ]
        }");

        _task.AvroGen = new ITaskItem[] 
        { 
            new TaskItem(schemaFile, new Hashtable 
            { 
                { "OutputFile", outputFile }
            })
        };

        try
        {
            // Act
            var result = _task.Execute();

            // Assert
            result.Should().BeTrue();
            File.Exists(outputFile).Should().BeTrue();
            var generatedCode = File.ReadAllText(outputFile);
            generatedCode.Should().Contain("namespace Test");
            generatedCode.Should().Contain("public class User");
            _errors.Should().BeEmpty();
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Execute_WithInvalidSchema_ShouldLogError()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var schemaFile = Path.Combine(tempDir, "invalid.avsc");
        var outputFile = Path.Combine(tempDir, "Invalid.cs");

        File.WriteAllText(schemaFile, @"{ ""type"": ""invalid"" }");

        _task.AvroGen = new ITaskItem[] 
        { 
            new TaskItem(schemaFile, new Hashtable 
            { 
                { "OutputFile", outputFile }
            })
        };

        try
        {
            // Act
            var result = _task.Execute();

            // Assert
            result.Should().BeFalse();
            File.Exists(outputFile).Should().BeFalse();
            _errors.Should().NotBeEmpty();
            _errors[0].Message.Should().Contain("invalid");
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Execute_WithMissingSchema_ShouldLogError()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), "nonexistent.avsc");

        _task.AvroGen = new ITaskItem[] 
        { 
            new TaskItem(nonExistentFile, new Hashtable 
            { 
                { "OutputFile", "output.cs" }
            })
        };

        // Act
        var result = _task.Execute();

        // Assert
        result.Should().BeFalse();
        _errors.Should().NotBeEmpty();
        _errors[0].Message.Should().Contain("not found");
    }

    [Fact]
    public void Execute_WithMultipleSchemas_ShouldGenerateAllFiles()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var schema1File = Path.Combine(tempDir, "user.avsc");
        var schema2File = Path.Combine(tempDir, "order.avsc");
        var output1File = Path.Combine(tempDir, "Test.User.cs");
        var output2File = Path.Combine(tempDir, "Test.Order.cs");

        File.WriteAllText(schema1File, @"{
            ""type"": ""record"",
            ""name"": ""User"",
            ""namespace"": ""Test"",
            ""fields"": [
                { ""name"": ""name"", ""type"": ""string"" }
            ]
        }");

        File.WriteAllText(schema2File, @"{
            ""type"": ""record"",
            ""name"": ""Order"",
            ""namespace"": ""Test"",
            ""fields"": [
                { ""name"": ""id"", ""type"": ""string"" }
            ]
        }");

        _task.AvroGen = new ITaskItem[] 
        { 
            new TaskItem(schema1File, new Hashtable { { "OutputFile", output1File } }),
            new TaskItem(schema2File, new Hashtable { { "OutputFile", output2File } })
        };

        try
        {
            // Act
            var result = _task.Execute();

            // Assert
            result.Should().BeTrue();
            File.Exists(output1File).Should().BeTrue();
            File.Exists(output2File).Should().BeTrue();
            _errors.Should().BeEmpty();

            var code1 = File.ReadAllText(output1File);
            var code2 = File.ReadAllText(output2File);

            code1.Should().Contain("public class User");
            code2.Should().Contain("public class Order");
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }
}
