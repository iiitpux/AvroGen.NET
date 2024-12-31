using Microsoft.Build.Framework;
using MSBuildTask = Microsoft.Build.Utilities.Task;
using Confluent.SchemaRegistry;

namespace AvroGen.NET
{
    /// <summary>
    /// MSBuild задача для генерации C# классов из Avro схем в Schema Registry.
    /// </summary>
    public class GenerateAvroClassesTask : MSBuildTask
    {
        private readonly ISchemaRegistryClient? _schemaRegistryClient;

        /// <summary>
        /// Инициализирует новый экземпляр задачи GenerateAvroClassesTask.
        /// </summary>
        public GenerateAvroClassesTask()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр задачи GenerateAvroClassesTask с указанным клиентом Schema Registry.
        /// </summary>
        /// <param name="schemaRegistryClient">Клиент Schema Registry для использования</param>
        public GenerateAvroClassesTask(ISchemaRegistryClient schemaRegistryClient)
        {
            _schemaRegistryClient = schemaRegistryClient;
        }

        /// <summary>
        /// Получает или устанавливает элементы схем для обработки.
        /// Каждый элемент должен иметь следующие метаданные:
        /// - SchemaRegistryUrl: URL Schema Registry
        /// - Subject: Тема схемы в Schema Registry
        /// - Version: Версия схемы в Schema Registry (необязательно)
        /// - OutputDirectory: Каталог для сгенерированных классов
        /// - Namespace: Пространство имен для сгенерированных классов (необязательно)
        /// </summary>
        [Required]
        public ITaskItem[] Schemas { get; set; } = Array.Empty<ITaskItem>();

        /// <summary>
        /// Выполняет задачу генерации C# классов из Avro схем.
        /// </summary>
        /// <returns>True, если выполнение задачи прошло успешно; иначе false.</returns>
        public override bool Execute()
        {
            try
            {
                Log.LogMessage(MessageImportance.High, "Начинаем генерацию Avro классов...");

                if (Schemas == null || Schemas.Length == 0)
                {
                    Log.LogError("Не указаны схемы для обработки");
                    return false;
                }

                foreach (var schema in Schemas)
                {
                    var schemaRegistryUrl = schema.GetMetadata("SchemaRegistryUrl");
                    var subject = schema.GetMetadata("Subject");
                    var outputDirectory = schema.GetMetadata("OutputDirectory");

                    if (string.IsNullOrEmpty(schemaRegistryUrl))
                    {
                        Log.LogError("Требуется указать SchemaRegistryUrl");
                        return false;
                    }

                    if (string.IsNullOrEmpty(subject))
                    {
                        Log.LogError("Требуется указать Subject");
                        return false;
                    }

                    if (string.IsNullOrEmpty(outputDirectory))
                    {
                        Log.LogError("Требуется указать OutputDirectory");
                        return false;
                    }

                    var config = new SchemaGeneratorConfig
                    {
                        SchemaRegistryUrl = schemaRegistryUrl,
                        Subject = subject,
                        OutputDirectory = outputDirectory,
                        Namespace = schema.GetMetadata("Namespace")
                    };

                    if (int.TryParse(schema.GetMetadata("Version"), out var version))
                    {
                        config.Version = version;
                    }

                    // Логируем детали конфигурации
                    LogConfiguration(schema, config);

                    // Создаем выходной каталог
                    Directory.CreateDirectory(config.OutputDirectory);

                    // Генерируем классы
                    var generator = _schemaRegistryClient != null
                        ? new SchemaGenerator(config, _schemaRegistryClient)
                        : new SchemaGenerator(config);

                    generator.GenerateAsync().Wait();
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex, true);
                return false;
            }
        }

        private void LogConfiguration(ITaskItem schema, SchemaGeneratorConfig config)
        {
            Log.LogMessage(MessageImportance.High, $"Обрабатываем схему: {schema.ItemSpec}");
            Log.LogMessage(MessageImportance.High, $"URL Schema Registry: {config.SchemaRegistryUrl}");
            Log.LogMessage(MessageImportance.High, $"Тема: {config.Subject}");
            Log.LogMessage(MessageImportance.High, $"Версия: {config.Version?.ToString() ?? "последняя"}");
            Log.LogMessage(MessageImportance.High, $"Выходной каталог: {config.OutputDirectory}");
            
            if (!string.IsNullOrEmpty(config.Namespace))
            {
                Log.LogMessage(MessageImportance.High, $"Пространство имен: {config.Namespace}");
            }
        }
    }
}
