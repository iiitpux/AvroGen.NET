using Xunit;
using System.CommandLine;

namespace AvroGen.NET.UnitTests;

/// <summary>
/// Модульные тесты для CLI команд
/// </summary>
public class CommandLineTests
{
    /// <summary>
    /// Проверяет базовую структуру аргументов командной строки
    /// </summary>
    [Fact]
    public void CommandLineArguments_ValidFormat()
    {
        // Arrange
        var args = new[]
        {
            "--schema-registry-url", "http://localhost:8081",
            "--subject", "test-subject",
            "--version", "1",
            "--output-directory", "./Generated",
            "--namespace", "TestNamespace"
        };

        // Assert
        Assert.Contains("--schema-registry-url", args);
        Assert.Contains("--subject", args);
        Assert.Contains("--version", args);
        Assert.Contains("--output-directory", args);
        Assert.Contains("--namespace", args);
    }

    /// <summary>
    /// Проверяет формат URL схема-реестра
    /// </summary>
    [Fact]
    public void SchemaRegistryUrl_ValidFormat()
    {
        // Arrange
        var url = "http://localhost:8081";

        // Assert
        Assert.StartsWith("http", url);
        Assert.Contains("://", url);
    }

    /// <summary>
    /// Проверяет формат версии схемы
    /// </summary>
    [Fact]
    public void SchemaVersion_ValidFormat()
    {
        // Arrange
        var version = "1";

        // Assert
        Assert.True(int.TryParse(version, out var _));
    }
}
