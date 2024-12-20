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
    public class SchemaGenerator
    {
        private readonly SchemaGeneratorConfig _config;
        private readonly CachedSchemaRegistryClient _schemaRegistry;

        public SchemaGenerator(SchemaGeneratorConfig config)
        {
            _config = config;
            _schemaRegistry = new CachedSchemaRegistryClient(new SchemaRegistryConfig
            {
                Url = config.SchemaRegistryUrl
            });
        }

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
