using AvroGen.NET;
using System.CommandLine;

var schemaRegistryUrlOption = new Option<string>(
    "--schema-registry-url",
    "The URL of the Schema Registry"
) { IsRequired = true };

var subjectOption = new Option<string>(
    "--subject",
    "The subject name in Schema Registry"
) { IsRequired = true };

var versionOption = new Option<int?>(
    "--schema-version",
    () => null,
    "The version of the schema (optional, defaults to latest)"
);

var outputOption = new Option<string>(
    "--output",
    () => "./Generated",
    "The output path for the generated class"
);

var namespaceOption = new Option<string>(
    "--namespace",
    () => null,
    "The namespace for the generated class (optional, defaults to subject name)"
);

var rootCommand = new RootCommand("Generates C# classes from Avro schemas in Schema Registry");
rootCommand.AddOption(schemaRegistryUrlOption);
rootCommand.AddOption(subjectOption);
rootCommand.AddOption(versionOption);
rootCommand.AddOption(outputOption);
rootCommand.AddOption(namespaceOption);

rootCommand.SetHandler(async (string schemaRegistryUrl, string subject, int? version, string output, string? namespaceName) =>
{
    try
    {
        var config = new SchemaGeneratorConfig
        {
            SchemaRegistryUrl = schemaRegistryUrl,
            OutputDirectory = output,
            Namespace = namespaceName,
            Subject = subject,
            Version = version
        };

        var generator = new SchemaGenerator(config);
        await generator.GenerateAsync();
        Console.WriteLine($"Successfully generated class for schema {subject} {(version.HasValue ? "version " + version.Value : "(latest version)")}");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
        Environment.Exit(1);
    }
}, schemaRegistryUrlOption, subjectOption, versionOption, outputOption, namespaceOption);

return await rootCommand.InvokeAsync(args);
