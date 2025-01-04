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

            // Создаем директорию для сгенерированных файлов, если её нет
            if (!Directory.Exists(_config.OutputDirectory))
            {
                Directory.CreateDirectory(_config.OutputDirectory);
            }

            // Парсим схему

            // Генерируем код для каждого класса в отдельный файл
            // var generatedFiles = _codeGenerator.GenerateCode(schema, _config.Namespace);

            var schemaString = registeredSchema.SchemaString;

            if (schemaString == null)
            {
                // Схема не нашлась в реестре
                throw new Exception("Schema not found in registry");
            }

            JObject schemaObject = JObject.Parse(schemaString);

// Получаем namespace
            string schemaNamespace = schemaObject["namespace"]?.ToString();

            var namespaceMapping = new Dictionary<string, string>();

// Теперь можно использовать schemaNamespace при необходимости
// Например, добавить его в namespaceMapping, если его там еще нет

            if (!namespaceMapping.Any(kvp => kvp.Key == schemaNamespace) && !string.IsNullOrEmpty(_config.Namespace))
            {
                // Добавляем маппинг, если его нет
                // Здесь вы можете задать правило для преобразования Avro namespace в C# namespace;
                namespaceMapping.Add(schemaNamespace, _config.Namespace);
            }


            var codegen = new CodeGen();
            codegen.AddSchema(schemaString, namespaceMapping);

            codegen.GenerateCode();
            codegen.WriteTypes(_config.OutputDirectory);
        }
    }
}