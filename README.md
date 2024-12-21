# AvroGen.NET

[Русская версия](README.ru.md)

AvroGen.NET is a library for automatic generation of C# classes from Avro schemas stored in Confluent Schema Registry. It provides both a programmatic API for integration into your applications and a command-line tool for quick class generation.

## Installation

### NuGet Package

```bash
dotnet add package AvroGen.NET
```

### Command Line Tool

```bash
dotnet tool install --global --add-source ./nupkg AvroGen.NET.Tool
```

## Usage

### Programmatic Way

```csharp
using AvroGen.NET;

var config = new SchemaGeneratorConfig
{
    SchemaRegistryUrl = "http://localhost:8081",
    OutputDirectory = "./generated"
};

var generator = new SchemaGenerator(config);
await generator.GenerateClassFromSchema("user-value", 1);
```

### Command Line Way

After installing the global tool, you can use it to generate classes:

```bash
avrogennet --schema-registry-url http://localhost:8081 --subject user-value --schema-version 1 --output ./generated
```

Parameters:
- `--schema-registry-url` - Schema Registry URL
- `--subject` - Schema subject name in Schema Registry
- `--schema-version` - Schema version
- `--output` - Path for saving generated class

## Features

- Automatic C# class generation from Avro schemas
- Integration with Schema Registry
- MSBuild task for seamless build process integration
- Support for schema versioning
- Clean and maintainable generated code
- NuGet package for easy distribution

## Configuration Options

- `Subject`: The Schema Registry subject name
- `Version`: The schema version to use (optional, defaults to latest)
- `SchemaRegistryUrl`: URL of your Schema Registry instance
- `OutputPath`: Directory where generated classes will be placed

## Example

Here's a complete example of how to use AvroGen.NET in your project:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <GeneratedCodePath>$(MSBuildProjectDirectory)\Generated</GeneratedCodePath>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="AvroGen.NET" Version="1.0.0" />
    <PackageReference Include="Confluent.SchemaRegistry" Version="2.3.0" />
    <PackageReference Include="Confluent.SchemaRegistry.Serdes.Avro" Version="2.3.0" />
  </ItemGroup>

  <ItemGroup>
    <AvroSchema Include="Schemas\test-schema.avsc">
      <Subject>test-schema-value</Subject>
      <Version>1</Version>
      <SchemaRegistryUrl>http://localhost:8081</SchemaRegistryUrl>
      <OutputPath>$(GeneratedCodePath)</OutputPath>
    </AvroSchema>
  </ItemGroup>
</Project>
```

## Requirements

- .NET 8.0 or later
- Access to a Schema Registry instance
- MSBuild 17.0 or later

## Building from Source

1. Clone the repository:
```bash
git clone https://github.com/iiitpux/AvroGen.NET.git
```

2. Build the solution:
```bash
dotnet build
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [Apache Avro](https://avro.apache.org/) for the Avro serialization system
- [Confluent Schema Registry](https://docs.confluent.io/platform/current/schema-registry/index.html) for schema management
- The .NET community for inspiration and support

## Local Development

### Requirements

- .NET 8.0 SDK
- Docker and Docker Compose (for local Schema Registry)

### Environment Setup

1. Start local Schema Registry:
```bash
cd infrastructure
./start.ps1
```

2. Stop local environment:
```bash
cd infrastructure
./stop.ps1
```

## Project Structure

- `src/` - source code
  - `AvroGen.NET/` - main library
  - `AvroGen.NET.Tool/` - command line tool
- `tests/` - tests
  - `AvroGen.NET.UnitTests/` - unit tests
  - `AvroGen.NET.IntegrationTests/` - integration tests
- `examples/` - usage examples
- `infrastructure/` - files for local development
- `docs/` - documentation
- `build/` - build artifacts
- `schemas/` - Avro schema examples

## Configuration Options

- `Subject`: Schema Registry subject name
- `Version`: Schema version
- `SchemaRegistryUrl`: Schema Registry URL
- `OutputPath`: Generated files path
- `Namespace`: Namespace for generated classes
- `GenerateAsync`: Generate async serialization methods
- `GenerateEquality`: Generate equality comparison methods
- `GenerateJsonMethods`: Generate JSON serialization methods

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## Acknowledgments

- [Apache Avro](https://avro.apache.org/) for the Avro serialization system
- [Confluent Schema Registry](https://docs.confluent.io/platform/current/schema-registry/index.html) for schema management
- The .NET community for inspiration and support

## License

MIT
