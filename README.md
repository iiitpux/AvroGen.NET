# AvroGen.NET

AvroGen.NET - это библиотека для автоматической генерации C# классов из Avro схем, хранящихся в Confluent Schema Registry. Она предоставляет как программный API для интеграции в ваши приложения, так и консольный инструмент для быстрой генерации классов.

## Установка

### NuGet пакет

```bash
dotnet add package AvroGen.NET
```

### Консольный инструмент

```bash
dotnet tool install --global --add-source ./nupkg AvroGen.NET.Tool
```

## Использование

### Программный способ

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

### Консольный способ

После установки глобального инструмента, вы можете использовать его для генерации классов:

```bash
avrogennet --schema-registry-url http://localhost:8081 --subject user-value --schema-version 1 --output ./generated
```

Параметры:
- `--schema-registry-url` - URL Schema Registry
- `--subject` - имя субъекта (схемы) в Schema Registry
- `--schema-version` - версия схемы
- `--output` - путь для сохранения сгенерированного класса

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

## Локальная разработка

### Требования

- .NET 8.0 SDK
- Docker и Docker Compose (для локального Schema Registry)

### Настройка окружения

1. Запустите локальный Schema Registry:
```bash
cd infrastructure
./start.ps1
```

2. Остановка локального окружения:
```bash
cd infrastructure
./stop.ps1
```

## Структура проекта

- `src/` - исходный код
  - `AvroGen.NET/` - основная библиотека
  - `AvroGen.NET.Tool/` - консольный инструмент
- `tests/` - тесты
  - `AvroGen.NET.UnitTests/` - модульные тесты
  - `AvroGen.NET.IntegrationTests/` - интеграционные тесты
- `examples/` - примеры использования
- `infrastructure/` - файлы для локальной разработки
- `docs/` - документация
- `build/` - артефакты сборки
- `schemas/` - примеры Avro схем

## Лицензия

MIT
