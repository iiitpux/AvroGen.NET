# AvroGen.NET

[English version](README.md)

AvroGen.NET - это инструмент для автоматической генерации C# классов из Avro схем, хранящихся в Confluent Schema Registry. Он интегрируется с MSBuild для автоматической генерации классов во время сборки проекта.

## Возможности

- Автоматическая генерация C# классов из Avro схем
- Интеграция с Confluent Schema Registry
- Интеграция с MSBuild для автоматической генерации кода при сборке
- Консольный инструмент для ручной генерации кода
- Поддержка всех типов данных Avro
- Настройка пространства имен для генерируемых классов
- Сгенерированные классы совместимы с Confluent.SchemaRegistry.Serdes.Avro

## Установка

### NuGet пакет (MSBuild интеграция)

```bash
dotnet add package AvroGen.NET
```

### Консольный инструмент

```bash
dotnet tool install -g AvroGen.NET.Tool
```

## Использование

### MSBuild интеграция (Рекомендуется)

Добавьте следующий код в файл проекта:

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

Классы будут генерироваться автоматически во время сборки. Вы также можете сгенерировать их вручную, выполнив команду:

```bash
dotnet build
```

### Консольный инструмент

Для разовой генерации или когда вам нужен больший контроль, вы можете использовать консольный инструмент:

```bash
# Генерация из последней версии схемы
avrogennet --schema-registry-url http://localhost:8081 --subject user-value --output ./Generated

# Генерация из конкретной версии схемы
avrogennet --schema-registry-url http://localhost:8081 --subject user-value --schema-version 1 --output ./Generated --namespace MyCompany.Models
```

Доступные опции:
- `--schema-registry-url` (обязательный): URL адрес Schema Registry
- `--subject` (обязательный): Имя схемы в Schema Registry
- `--schema-version` (необязательный): Версия схемы (по умолчанию используется последняя)
- `--output` (необязательный): Директория для вывода (по умолчанию ./Generated)
- `--namespace` (необязательный): Пространство имен для генерируемых классов (по умолчанию используется имя схемы)

## Настройка

Элемент `AvroSchema` поддерживает следующие параметры:

- `SchemaRegistryUrl` - URL адрес Schema Registry
- `Subject` - Имя схемы в Schema Registry
- `Version` - Версия схемы (необязательно, по умолчанию используется последняя версия)
- `OutputDirectory` - Директория для сгенерированных классов
- `Namespace` - Пространство имен для сгенерированных классов (необязательно, по умолчанию используется имя схемы)

## Пример

Посмотрите [пример проекта](examples/AvroGen.NET.Example) для полного рабочего примера.

## Как это работает

1. Во время сборки AvroGen.NET подключается к указанному Schema Registry
2. Получает Avro схему для указанного subject и версии
3. Генерирует C# классы, соответствующие структуре схемы
4. Помещает сгенерированные файлы в указанную директорию
5. Включает сгенерированные файлы в компиляцию

Сгенерированные классы:
- Реализуют интерфейс `ISpecificRecord` из Apache.Avro
- Поддерживают сериализацию/десериализацию с помощью Confluent.SchemaRegistry.Serdes.Avro
- Включают все необходимые свойства и типы данных из Avro схемы
- Сохраняют совместимость схем для бесшовной интеграции с Kafka

## Технические детали

### MSBuild интеграция

AvroGen.NET интегрируется с MSBuild через:
- Файлы `.props` и `.targets` для определения задач сборки
- Задачу `GenerateAvroClassesTask` для генерации классов
- Автоматическое включение сгенерированных файлов в компиляцию

### Генерация кода

Процесс генерации включает:
1. Получение схемы из Schema Registry
2. Парсинг схемы с помощью Apache.Avro
3. Генерация C# классов с помощью CodeDom
4. Добавление атрибутов для совместимости с Confluent.SchemaRegistry.Serdes.Avro

## Лицензия

MIT
