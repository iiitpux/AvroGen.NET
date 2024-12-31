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

            // Парсим схему и генерируем код
            var generator = new CodeGen();
            if (!string.IsNullOrEmpty(_config.Namespace))
            {
                generator.AddSchema(registeredSchema.SchemaString, new Dictionary<string, string> 
                { 
                    { "namespace", _config.Namespace } 
                });
            }
            else
            {
                generator.AddSchema(registeredSchema.SchemaString);
            }

            var code = generator.GenerateCode();

            // Создаем выходной каталог, если он не существует
            Directory.CreateDirectory(_config.OutputDirectory);

            // Определяем имя файла
            var schema = Avro.Schema.Parse(registeredSchema.SchemaString);
            var fileName = $"{schema.Name}.cs";
            var filePath = Path.Combine(_config.OutputDirectory, fileName);

            // Сохраняем сгенерированный код
            using (var provider = new CSharpCodeProvider())
            using (var writer = new StreamWriter(filePath))
            {
                var options = new CodeGeneratorOptions
                {
                    BracingStyle = "C"
                };

                provider.GenerateCodeFromCompileUnit(code, writer, options);
            }
        }
    }
}
