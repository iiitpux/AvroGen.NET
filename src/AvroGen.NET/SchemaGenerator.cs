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
    /// Generator for creating C# classes from Avro schemas in Schema Registry.
    /// </summary>
    public class SchemaGenerator
    {
        private readonly SchemaGeneratorConfig _config;
        private readonly CachedSchemaRegistryClient _schemaRegistry;

        /// <summary>
        /// Initializes a new instance of the SchemaGenerator class.
        /// </summary>
        /// <param name="config">Configuration for schema generation.</param>
        public SchemaGenerator(SchemaGeneratorConfig config)
        {
            _config = config;
            _schemaRegistry = new CachedSchemaRegistryClient(new SchemaRegistryConfig
            {
                Url = config.SchemaRegistryUrl
            });
        }

        /// <summary>
        /// Generates C# classes from the configured Schema Registry schema.
        /// </summary>
        public async Task GenerateAsync()
        {
            // Get schema from Schema Registry
            var registeredSchema = _config.Version.HasValue
                ? await _schemaRegistry.GetRegisteredSchemaAsync(_config.Subject, _config.Version.Value)
                : await _schemaRegistry.GetLatestSchemaAsync(_config.Subject);

            // Parse schema and generate code
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

            // Create output directory if it doesn't exist
            Directory.CreateDirectory(_config.OutputDirectory);

            // Generate output file path
            var schema = Avro.Schema.Parse(registeredSchema.SchemaString);
            var outputPath = Path.Combine(_config.OutputDirectory, $"{schema.Name}.cs");

            // Write the generated code
            using (var provider = new CSharpCodeProvider())
            using (var writer = new StreamWriter(outputPath))
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
