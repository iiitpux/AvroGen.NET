# AvroGen.NET

[English version](README.md)

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
