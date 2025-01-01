using Confluent.SchemaRegistry;
using Avro;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AvroGen.NET
{
    /// <summary>
    /// Генератор C# классов из Avro схем в Schema Registry.
    /// </summary>
    public class SchemaGenerator
    {
        private readonly SchemaGeneratorConfig _config;
        private readonly ISchemaRegistryClient _schemaRegistryClient;
        private readonly AvroCodeGenerator _codeGenerator;

        /// <summary>
        /// Создает новый экземпляр генератора схем.
        /// </summary>
        /// <param name="config">Конфигурация генератора</param>
        public SchemaGenerator(SchemaGeneratorConfig config)
            : this(config, new CachedSchemaRegistryClient(new SchemaRegistryConfig { Url = config.SchemaRegistryUrl }))
        {
        }

        /// <summary>
        /// Создает новый экземпляр генератора схем с пользовательским клиентом Schema Registry.
        /// </summary>
        /// <param name="config">Конфигурация генератора</param>
        /// <param name="schemaRegistryClient">Клиент Schema Registry</param>
        public SchemaGenerator(SchemaGeneratorConfig config, ISchemaRegistryClient schemaRegistryClient)
        {
            _config = config;
            _schemaRegistryClient = schemaRegistryClient;
            _codeGenerator = new AvroCodeGenerator();
        }

        /// <summary>
        /// Генерирует C# классы из Avro схемы.
        /// </summary>
        public async Task GenerateAsync()
        {
            // Получаем схему из реестра
            var registeredSchema = _config.Version.HasValue
                ? await _schemaRegistryClient.GetRegisteredSchemaAsync(_config.Subject, _config.Version.Value)
                : await _schemaRegistryClient.GetLatestSchemaAsync(_config.Subject);

            // Создаем директорию для сгенерированных файлов, если её нет
            if (!Directory.Exists(_config.OutputDirectory))
            {
                Directory.CreateDirectory(_config.OutputDirectory);
            }

            // Парсим схему
            var schema = Avro.Schema.Parse(registeredSchema.SchemaString);

            // Генерируем код для каждого класса в отдельный файл
            var generatedFiles = _codeGenerator.GenerateCode(schema, _config.Namespace);

            // Сохраняем каждый класс в отдельный файл
            foreach (var file in generatedFiles)
            {
                var filePath = Path.Combine(_config.OutputDirectory, file.Key);
                await File.WriteAllTextAsync(filePath, file.Value);
            }
        }
    }
}
