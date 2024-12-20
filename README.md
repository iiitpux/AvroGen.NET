# AvroGen.NET

AvroGen.NET is an MSBuild task that automatically generates C# classes from Avro schemas stored in a Schema Registry. It simplifies the process of working with Avro schemas in .NET applications by integrating directly into your build process.

## Features

- Automatic C# class generation from Avro schemas
- Integration with Schema Registry
- MSBuild task for seamless build process integration
- Support for schema versioning
- Clean and maintainable generated code
- NuGet package for easy distribution

## Installation

Install the NuGet package in your project:

```bash
dotnet add package AvroGen.NET
```

## Usage

1. Add the Schema Registry URL and schema details to your project file:

```xml
<ItemGroup>
  <AvroSchema Include="Schemas\your-schema.avsc">
    <Subject>your-schema-subject</Subject>
    <Version>1</Version>
    <SchemaRegistryUrl>http://localhost:8081</SchemaRegistryUrl>
    <OutputPath>$(MSBuildProjectDirectory)\Generated</OutputPath>
  </AvroSchema>
</ItemGroup>
```

2. Build your project. The C# classes will be automatically generated in the specified output directory.

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
