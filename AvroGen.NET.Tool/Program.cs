using AvroGen.NET;
using System.CommandLine;

var schemaRegistryUrlOption = new Option<string>(
    "--schema-registry-url",
    "The URL of the Schema Registry"
);

var subjectOption = new Option<string>(
    "--subject",
    "The subject name in Schema Registry"
);

var versionOption = new Option<int>(
    "--schema-version",
    "The version of the schema"
);

var outputOption = new Option<string>(
    "--output",
    "The output path for the generated class"
);

var rootCommand = new RootCommand("Generates C# classes from Avro schemas in Schema Registry");
rootCommand.AddOption(schemaRegistryUrlOption);
rootCommand.AddOption(subjectOption);
rootCommand.AddOption(versionOption);
rootCommand.AddOption(outputOption);

rootCommand.SetHandler(async (string schemaRegistryUrl, string subject, int version, string output) =>
{
    try
    {
        var config = new SchemaGeneratorConfig
        {
            SchemaRegistryUrl = schemaRegistryUrl,
            OutputDirectory = output
        };

        var generator = new SchemaGenerator(config);
        await generator.GenerateClassFromSchema(subject, version);
        Console.WriteLine($"Successfully generated class for schema {subject} version {version}");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
        Environment.Exit(1);
    }
}, schemaRegistryUrlOption, subjectOption, versionOption, outputOption);

return await rootCommand.InvokeAsync(args);
