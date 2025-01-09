namespace AvroGen.NET
{
    /// <summary>
    /// Конфигурация генератора схем.
    /// </summary>
    public class SchemaGeneratorConfig
    {
        /// <summary>
        /// Получает или устанавливает URL Schema Registry.
        /// </summary>
        public required string SchemaRegistryUrl { get; set; }

        /// <summary>
        /// Получает или устанавливает имя субъекта схемы в Schema Registry.
        /// </summary>
        public required string Subject { get; set; }

        /// <summary>
        /// Получает или устанавливает версию схемы в Schema Registry. 
        /// Если не указано, будет использована последняя версия.
        /// </summary>
        public int? Version { get; set; }

        /// <summary>
        /// Получает или устанавливает директорию для сгенерированных классов.
        /// </summary>
        public required string OutputDirectory { get; set; }

        /// <summary>
        /// Получает или устанавливает пространство имен для сгенерированных классов.
        /// </summary>
        public string? Namespace { get; set; }

        /// <summary>
        /// Создавать ли структуру директорий по пространствам имен.
        /// </summary>
        public bool CreateDirectoryStructure { get; set; } = true;
    }
}
