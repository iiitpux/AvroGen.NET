using System.CommandLine;
using AvroGen.NET;

namespace AvroGen.NET.Tool;

/// <summary>
/// Точка входа для консольного приложения AvroGen.NET.Tool.
/// </summary>
public class Program
{
    /// <summary>
    /// Основная точка входа приложения.
    /// </summary>
    /// <param name="args">Аргументы командной строки</param>
    /// <returns>Код возврата: 0 - успех, не 0 - ошибка</returns>
    public static async Task<int> Main(string[] args)
    {
        // Создаем корневую команду
        var rootCommand = new RootCommand("Генератор C# классов из Avro схем в Schema Registry");

        // Добавляем команду generate
        var generateCommand = new Command("generate", "Генерирует C# классы из Avro схем");

        // Опции команды
        var schemaRegistryUrlOption = new Option<string>(
            "--schema-registry-url",
            "URL Schema Registry")
        { IsRequired = true };

        var subjectOption = new Option<string>(
            "--subject",
            "Тема схемы в Schema Registry")
        { IsRequired = true };

        var versionOption = new Option<int?>(
            "--version",
            "Версия схемы (необязательно, по умолчанию последняя)");

        var outputDirOption = new Option<string>(
            "--output-dir",
            "Каталог для сгенерированных файлов")
        { IsRequired = true };

        var namespaceOption = new Option<string>(
            "--namespace",
            "Пространство имен для сгенерированных классов (необязательно)");

        // Добавляем опции к команде
        generateCommand.AddOption(schemaRegistryUrlOption);
        generateCommand.AddOption(subjectOption);
        generateCommand.AddOption(versionOption);
        generateCommand.AddOption(outputDirOption);
        generateCommand.AddOption(namespaceOption);

        // Обработчик команды generate
        generateCommand.SetHandler(async (string schemaRegistryUrl, string subject, int? version, string outputDir, string? @namespace) =>
        {
            try
            {
                // Создаем конфигурацию
                var config = new SchemaGeneratorConfig
                {
                    SchemaRegistryUrl = schemaRegistryUrl,
                    Subject = subject,
                    Version = version,
                    OutputDirectory = outputDir,
                    Namespace = @namespace
                };

                // Создаем генератор и запускаем генерацию
                var generator = new SchemaGenerator(config);
                await generator.GenerateAsync();

                Console.WriteLine("Генерация завершена успешно");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Ошибка: {ex.Message}");
                Environment.Exit(1);
            }
        },
        schemaRegistryUrlOption, subjectOption, versionOption, outputDirOption, namespaceOption);

        // Добавляем команду generate к корневой команде
        rootCommand.AddCommand(generateCommand);

        // Запускаем приложение
        return await rootCommand.InvokeAsync(args);
    }
}
