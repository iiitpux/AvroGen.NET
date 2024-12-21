# AvroGen.NET

[Русская версия](README.ru.md)

AvroGen.NET is a library for automatic generation of C# classes from Avro schemas stored in Confluent Schema Registry. It seamlessly integrates with MSBuild to automatically generate classes during build, and also provides a command-line tool and programmatic API for more specific use cases.

## Installation

### Library

```bash
dotnet add package AvroGen.NET
```

### Command Line Tool

```bash
dotnet tool install -g AvroGen.NET.Tool
```

## Usage

### MSBuild Integration (Recommended)

The recommended way to use AvroGen.NET is through MSBuild integration. This approach automatically generates classes during build time and ensures they are always in sync with your schemas.

Add the following to your project file:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <GeneratedCodePath>$(MSBuildProjectDirectory)/Generated</GeneratedCodePath>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="AvroGen.NET" Version="0.2.0" />
  </ItemGroup>

  <ItemGroup>
    <AvroSchema Include="test-schema">
      <Subject>test-schema-value</Subject>
      <Version>1</Version>
      <SchemaRegistryUrl>http://localhost:8081</SchemaRegistryUrl>
      <OutputPath>$(GeneratedCodePath)</OutputPath>
    </AvroSchema>
  </ItemGroup>
</Project>
```

Now just build your project and the classes will be generated automatically:
```bash
dotnet build
```

### Command Line Tool

For one-off generation or when you need more control, you can use the command-line tool:

```bash
avrogen-net generate --schema-registry-url http://localhost:8081 --subject test-schema-value --version 1 --output ./Generated
```

### Programmatic API

For advanced scenarios or when you need to integrate class generation into your application:

```csharp
using AvroGen.NET;

var config = new SchemaGeneratorConfig
{
    SchemaRegistryUrl = "http://localhost:8081",
    OutputDirectory = "./Generated"
};

var generator = new SchemaGenerator(config);
await generator.GenerateClassFromSchema("test-schema-value", 1);
```

## Features

- **MSBuild Integration**: Automatically generate classes during build
- **Command Line Tool**: Quick class generation for development and testing
- **Programmatic API**: Full control over the generation process
- **Schema Registry Integration**: Direct integration with Confluent Schema Registry
- **Clean Code Generation**: Generated classes follow C# best practices

## License

MIT
