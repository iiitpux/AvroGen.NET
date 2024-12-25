# AvroGen.NET

MSBuild-based tool for generating C# classes from Avro schemas stored in Schema Registry.

[Russian version](README.ru.md)

## Features

- Automatic C# class generation from Avro schemas
- Integration with Confluent Schema Registry
- MSBuild integration for automatic code generation during build
- Command-line tool for manual code generation
- Support for all Avro data types
- Namespace customization
- Generated classes are compatible with Confluent.SchemaRegistry.Serdes.Avro

## Installation

### NuGet Package (MSBuild Integration)

```bash
dotnet add package AvroGen.NET
```

### Command Line Tool

```bash
dotnet tool install -g AvroGen.NET.Tool
```

## Usage

### MSBuild Integration (Recommended)

Add the following to your project file:

```xml
<ItemGroup>
  <AvroSchema Include=".">
    <SchemaRegistryUrl>http://localhost:8081</SchemaRegistryUrl>
    <Subject>your-schema-subject</Subject>
    <Version>1</Version>
    <OutputDirectory>$(MSBuildProjectDirectory)\Generated</OutputDirectory>
    <Namespace>$(RootNamespace).Generated</Namespace>
  </AvroSchema>
</ItemGroup>
```

The classes will be generated automatically during build. You can also generate them manually by running:

```bash
dotnet build
```

### Command Line Tool

For one-time generation or when you need more control, you can use the command line tool:

```bash
# Generate from latest schema version
avrogennet --schema-registry-url http://localhost:8081 --subject user-value --output ./Generated

# Generate from specific schema version
avrogennet --schema-registry-url http://localhost:8081 --subject user-value --schema-version 1 --output ./Generated --namespace MyCompany.Models
```

Available options:
- `--schema-registry-url` (required): URL of the Schema Registry
- `--subject` (required): Schema Registry subject name
- `--schema-version` (optional): Schema version (defaults to latest)
- `--output` (optional): Output directory (defaults to ./Generated)
- `--namespace` (optional): Namespace for generated classes (defaults to subject name)

## Configuration

The `AvroSchema` item supports the following metadata:

- `SchemaRegistryUrl` - URL of the Schema Registry
- `Subject` - Schema Registry subject name
- `Version` - Schema version (optional, defaults to latest)
- `OutputDirectory` - Directory where the generated classes will be placed
- `Namespace` - Namespace for generated classes (optional, defaults to subject name)

## Example

Check out the [example project](examples/AvroGen.NET.Example) for a complete working example.

## How it works

1. During build, AvroGen.NET connects to the specified Schema Registry
2. Retrieves the Avro schema for the specified subject and version
3. Generates C# classes that match the schema structure
4. Places generated files in the specified output directory
5. Includes generated files in compilation

Generated classes:
- Implement `ISpecificRecord` interface from Apache.Avro
- Support serialization/deserialization with Confluent.SchemaRegistry.Serdes.Avro
- Include all necessary properties and data types from the Avro schema
- Maintain schema compatibility for seamless integration with Kafka

## License

MIT
