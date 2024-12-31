# AvroGen.NET

Библиотека для генерации C# классов из Avro схем в Schema Registry.

## Описание

AvroGen.NET - это библиотека, которая позволяет автоматически генерировать C# классы из Avro схем, хранящихся в Schema Registry. Библиотека интегрируется с MSBuild и может использоваться как часть процесса сборки проекта.

## Основные компоненты

### SchemaGenerator

Класс для генерации C# кода из Avro схем. Основные возможности:
- Получение схем из Schema Registry
- Поддержка конкретных версий схем или последней версии
- Генерация C# классов с поддержкой сериализации/десериализации

### AvroCodeGenerator

Класс для преобразования Avro схем в C# код. Возможности:
- Поддержка всех типов Avro (строки, числа, массивы, записи и т.д.)
- Генерация классов с правильными типами C#
- Поддержка вложенных схем и ссылок

### GenerateAvroClassesTask

MSBuild таск для интеграции с процессом сборки. Функции:
- Конфигурация через MSBuild properties
- Поддержка множественных схем
- Гибкая настройка выходного каталога и пространства имен

## Использование

1. Добавьте пакет AvroGen.NET в ваш проект
2. Настройте таск в файле .csproj:
```xml
<ItemGroup>
    <AvroSchema Include="MySchema">
        <SchemaRegistryUrl>http://localhost:8081</SchemaRegistryUrl>
        <Subject>my-schema</Subject>
        <OutputDirectory>$(MSBuildProjectDirectory)\Generated</OutputDirectory>
        <Namespace>MyNamespace</Namespace>
    </AvroSchema>
</ItemGroup>
```
3. При сборке проекта классы будут автоматически сгенерированы

## Требования

- .NET 8.0 или выше
- Доступ к Schema Registry
- MSBuild 17.0 или выше (для использования как MSBuild таск)
