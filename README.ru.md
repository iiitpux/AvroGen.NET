# AvroGen.NET

[English version](README.md)

AvroGen.NET - это библиотека для автоматической генерации C# классов из Avro схем, хранящихся в Confluent Schema Registry. Она тесно интегрируется с MSBuild для автоматической генерации классов во время сборки, а также предоставляет консольный инструмент и программный API для более специфических случаев использования.

## Установка

### Библиотека

```bash
dotnet add package AvroGen.NET
```

### Консольный инструмент

```bash
dotnet tool install -g AvroGen.NET.Tool
```

## Использование

### MSBuild интеграция (Рекомендуется)

Рекомендуемый способ использования AvroGen.NET - через интеграцию с MSBuild. Этот подход автоматически генерирует классы во время сборки и гарантирует, что они всегда синхронизированы с вашими схемами.

Добавьте следующее в ваш проектный файл:

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

Теперь просто соберите ваш проект, и классы будут сгенерированы автоматически:
```bash
dotnet build
```

### Консольный инструмент

Для разовой генерации или когда вам нужен больший контроль, вы можете использовать консольный инструмент:

```bash
avrogen-net generate --schema-registry-url http://localhost:8081 --subject test-schema-value --version 1 --output ./Generated
```

### Программный API

Для продвинутых сценариев или когда вам нужно интегрировать генерацию классов в ваше приложение:

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

## Возможности

- **MSBuild интеграция**: Автоматическая генерация классов во время сборки
- **Консольный инструмент**: Быстрая генерация классов для разработки и тестирования
- **Программный API**: Полный контроль над процессом генерации
- **Интеграция с Schema Registry**: Прямая интеграция с Confluent Schema Registry
- **Чистая генерация кода**: Сгенерированные классы следуют лучшим практикам C#

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

## Параметры конфигурации

- `Subject`: Имя субъекта в Schema Registry
- `Version`: Версия схемы
- `SchemaRegistryUrl`: URL Schema Registry
- `OutputPath`: Путь для сгенерированных файлов
- `Namespace`: Пространство имен для сгенерированных классов
- `GenerateAsync`: Генерировать асинхронные методы сериализации
- `GenerateEquality`: Генерировать методы сравнения на равенство
- `GenerateJsonMethods`: Генерировать методы JSON сериализации

## Участие в разработке

1. Сделайте форк репозитория
2. Создайте ветку для новой функциональности
3. Зафиксируйте ваши изменения
4. Отправьте изменения в ветку
5. Создайте Pull Request

## Благодарности

- [Apache Avro](https://avro.apache.org/) за систему сериализации Avro
- [Confluent Schema Registry](https://docs.confluent.io/platform/current/schema-registry/index.html) за управление схемами
- .NET сообществу за вдохновение и поддержку

## Лицензия

MIT
