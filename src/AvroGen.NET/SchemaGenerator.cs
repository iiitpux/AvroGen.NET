using Confluent.SchemaRegistry;
using Avro;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace AvroGen.NET
{
    /// <summary>
    /// Генератор C# классов из Avro схем в Schema Registry.
    /// </summary>
    public class SchemaGenerator
    {
        private readonly SchemaGeneratorConfig _config;
        private readonly ISchemaRegistryClient _schemaRegistryClient;

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

            if (string.IsNullOrEmpty(registeredSchema?.SchemaString))
            {
                throw new Exception("Schema not found in registry");
            }

            // Удаляем директорию перед генерацией файлов
            if (Directory.Exists(_config.OutputDirectory))
            {
                Directory.Delete(_config.OutputDirectory, true);
            }
            Directory.CreateDirectory(_config.OutputDirectory);

            var schemaObject = JObject.Parse(registeredSchema.SchemaString);
            string schemaNamespace = schemaObject["namespace"]?.ToString();

            var namespaceMapping = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(schemaNamespace) && !string.IsNullOrEmpty(_config.Namespace))
            {
                namespaceMapping.Add(schemaNamespace, _config.Namespace);
            }

            // Используем VersionedCodeGen вместо CodeGen
            var codegen = new VersionedCodeGen(registeredSchema.Version);
            codegen.AddSchema(registeredSchema.SchemaString, namespaceMapping);

            // Генерируем код
            codegen.GenerateCode();
            codegen.WriteTypes(_config.OutputDirectory, !_config.CreateDirectoryStructure);
        }
    }
}