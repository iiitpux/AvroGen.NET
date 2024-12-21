using Confluent.SchemaRegistry;
using Avro;
using System.Text;
using AvroSchema = Avro.Schema;
using System;
using System.IO;
using System.Threading.Tasks;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace AvroGen.NET
{
    /// <summary>
    /// Generator for creating C# classes from Avro schemas stored in Schema Registry.
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
        /// Generates a C# class from the specified Avro schema.
        /// </summary>
        /// <param name="subject">The subject of the schema.</param>
        /// <param name="version">The version of the schema.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GenerateClassFromSchema(string subject, int version)
        {
            var schema = await _schemaRegistry.GetRegisteredSchemaAsync(subject, version);
            var avroSchema = AvroSchema.Parse(schema.SchemaString);

            var generator = new CodeGen();
            generator.AddSchema(avroSchema);
            var code = generator.GenerateCode();

            var outputPath = Path.Combine(_config.OutputDirectory, $"{avroSchema.Name}.cs");
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
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
