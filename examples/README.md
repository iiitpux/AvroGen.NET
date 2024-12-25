# Examples

[Русская версия](README.ru.md)

This directory contains example projects demonstrating how to use AvroGen.NET.

## Projects

### AvroGen.NET.Example

A simple example showing how to:
- Configure AvroGen.NET in a project
- Generate C# classes from Avro schemas
- Use generated classes with Confluent.SchemaRegistry.Serdes.Avro

### Prerequisites

1. Start the local infrastructure:
```powershell
cd ..\infrastructure
.\start.ps1
```

2. Build the example:
```powershell
dotnet build
```

The generated classes will be placed in the `Generated` directory.
