using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Moq;
using System.Reflection;
using Xunit;
using Confluent.SchemaRegistry;

namespace AvroGen.NET.UnitTests;

/// <summary>
/// Unit tests for GenerateAvroClassesTask class
/// </summary>
public class GenerateAvroClassesTaskTests
{
    private readonly Mock<IBuildEngine> _buildEngineMock;
    private readonly Mock<ISchemaRegistryClient> _schemaRegistryMock;
    private readonly GenerateAvroClassesTask _task;
    private const string TestSchema = @"{
        ""type"": ""record"",
        ""name"": ""TestRecord"",
        ""namespace"": ""Test"",
        ""fields"": [
            { ""name"": ""field1"", ""type"": ""string"" },
            { ""name"": ""field2"", ""type"": ""int"" }
        ]
    }";

    public GenerateAvroClassesTaskTests()
    {
        _buildEngineMock = new Mock<IBuildEngine>();
        _schemaRegistryMock = new Mock<ISchemaRegistryClient>();
        _task = new GenerateAvroClassesTask(_schemaRegistryMock.Object)
        {
            BuildEngine = _buildEngineMock.Object
        };
    }

    /// <summary>
    /// Tests validation of required parameters
    /// </summary>
    [Fact]
    public void Execute_MissingSchemas_ReturnsFalse()
    {
        // Arrange
        _task.Schemas = Array.Empty<ITaskItem>();

        // Act
        var result = _task.Execute();

        // Assert
        Assert.False(result);
        _buildEngineMock.Verify(x => x.LogErrorEvent(It.Is<BuildErrorEventArgs>(
            e => e.Message.Contains("No schemas specified for processing"))), Times.Once);
    }

    /// <summary>
    /// Tests successful task execution with valid parameters
    /// </summary>
    [Fact]
    public void Execute_WithValidParameters_ReturnsTrue()
    {
        // Arrange
        var taskItem = new Mock<ITaskItem>();
        taskItem.Setup(x => x.GetMetadata("SchemaRegistryUrl")).Returns("http://localhost:8081");
        taskItem.Setup(x => x.GetMetadata("Subject")).Returns("test-subject");
        taskItem.Setup(x => x.GetMetadata("OutputDirectory")).Returns(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        taskItem.Setup(x => x.GetMetadata("Version")).Returns("1");
        taskItem.Setup(x => x.GetMetadata("Namespace")).Returns("TestNamespace");
        taskItem.Setup(x => x.ItemSpec).Returns("test-subject");

        _task.Schemas = new[] { taskItem.Object };

        var registeredSchema = new RegisteredSchema("test-subject", 1, 1, TestSchema, SchemaType.Avro, new List<SchemaReference>());
        _schemaRegistryMock
            .Setup(x => x.GetRegisteredSchemaAsync("test-subject", 1))
            .ReturnsAsync(registeredSchema);

        try
        {
            // Act
            var result = _task.Execute();

            // Assert
            Assert.True(result);
            _buildEngineMock.Verify(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>()), Times.Never);
        }
        finally
        {
            if (!string.IsNullOrEmpty(taskItem.Object.GetMetadata("OutputDirectory")) &&
                Directory.Exists(taskItem.Object.GetMetadata("OutputDirectory")))
            {
                Directory.Delete(taskItem.Object.GetMetadata("OutputDirectory"), true);
            }
        }
    }

    /// <summary>
    /// Tests handling of missing schema registry URL
    /// </summary>
    [Fact]
    public void Execute_MissingSchemaRegistryUrl_ReturnsFalse()
    {
        // Arrange
        var taskItem = new Mock<ITaskItem>();
        taskItem.Setup(x => x.GetMetadata("SchemaRegistryUrl")).Returns("");
        taskItem.Setup(x => x.GetMetadata("Subject")).Returns("test-subject");
        taskItem.Setup(x => x.GetMetadata("OutputDirectory")).Returns("TestOutput");

        _task.Schemas = new[] { taskItem.Object };

        // Act
        var result = _task.Execute();

        // Assert
        Assert.False(result);
        _buildEngineMock.Verify(x => x.LogErrorEvent(It.Is<BuildErrorEventArgs>(
            e => e.Message.Contains("SchemaRegistryUrl is required"))), Times.Once);
    }

    /// <summary>
    /// Tests handling of missing subject
    /// </summary>
    [Fact]
    public void Execute_MissingSubject_ReturnsFalse()
    {
        // Arrange
        var taskItem = new Mock<ITaskItem>();
        taskItem.Setup(x => x.GetMetadata("SchemaRegistryUrl")).Returns("http://localhost:8081");
        taskItem.Setup(x => x.GetMetadata("Subject")).Returns("");
        taskItem.Setup(x => x.GetMetadata("OutputDirectory")).Returns("TestOutput");

        _task.Schemas = new[] { taskItem.Object };

        // Act
        var result = _task.Execute();

        // Assert
        Assert.False(result);
        _buildEngineMock.Verify(x => x.LogErrorEvent(It.Is<BuildErrorEventArgs>(
            e => e.Message.Contains("Subject is required"))), Times.Once);
    }

    /// <summary>
    /// Tests handling of missing output directory
    /// </summary>
    [Fact]
    public void Execute_MissingOutputDirectory_ReturnsFalse()
    {
        // Arrange
        var taskItem = new Mock<ITaskItem>();
        taskItem.Setup(x => x.GetMetadata("SchemaRegistryUrl")).Returns("http://localhost:8081");
        taskItem.Setup(x => x.GetMetadata("Subject")).Returns("test-subject");
        taskItem.Setup(x => x.GetMetadata("OutputDirectory")).Returns("");

        _task.Schemas = new[] { taskItem.Object };

        // Act
        var result = _task.Execute();

        // Assert
        Assert.False(result);
        _buildEngineMock.Verify(x => x.LogErrorEvent(It.Is<BuildErrorEventArgs>(
            e => e.Message.Contains("OutputDirectory is required"))), Times.Once);
    }

    /// <summary>
    /// Tests creation of output directory if it does not exist
    /// </summary>
    [Fact]
    public void Execute_CreatesOutputDirectory_IfNotExists()
    {
        // Arrange
        var outputDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var taskItem = new Mock<ITaskItem>();
        taskItem.Setup(x => x.GetMetadata("SchemaRegistryUrl")).Returns("http://localhost:8081");
        taskItem.Setup(x => x.GetMetadata("Subject")).Returns("test-subject");
        taskItem.Setup(x => x.GetMetadata("OutputDirectory")).Returns(outputDir);
        taskItem.Setup(x => x.GetMetadata("Version")).Returns("1");
        taskItem.Setup(x => x.GetMetadata("Namespace")).Returns("TestNamespace");

        _task.Schemas = new[] { taskItem.Object };

        var registeredSchema = new RegisteredSchema("test-subject", 1, 1, TestSchema, SchemaType.Avro, new List<SchemaReference>());
        _schemaRegistryMock
            .Setup(x => x.GetRegisteredSchemaAsync("test-subject", 1))
            .ReturnsAsync(registeredSchema);

        try
        {
            // Act
            var result = _task.Execute();

            // Assert
            Assert.True(Directory.Exists(outputDir));
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
            }
        }
    }
}
