# AvroGen.NET

MSBuild-based tool for generating C# classes from Avro schemas stored in Schema Registry.

## Installation

Install the NuGet package:

```bash
dotnet add package AvroGen.NET
```

## Usage

Add the following to your project file:

```xml
<ItemGroup>
  <AvroGen Include=".">
    <Subject>your-schema-subject</Subject>
    <Version>1</Version>
    <SchemaRegistryUrl>http://localhost:8081</SchemaRegistryUrl>
    <OutputPath>$(MSBuildProjectDirectory)/Generated</OutputPath>
  </AvroGen>
</ItemGroup>
```

The classes will be generated during build. You can also generate them manually by running:

```bash
dotnet build
```

## Configuration

The `AvroGen` item supports the following metadata:

- `Subject` - Schema Registry subject name
- `Version` - Schema version (optional, defaults to latest)
- `SchemaRegistryUrl` - URL of the Schema Registry
- `OutputPath` - Directory where the generated classes will be placed

## License

MIT
