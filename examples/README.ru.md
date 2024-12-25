# Примеры

[English version](README.md)

В этой директории находятся примеры проектов, демонстрирующие использование AvroGen.NET.

## Проекты

### AvroGen.NET.Example

Простой пример, показывающий как:
- Настроить AvroGen.NET в проекте
- Генерировать C# классы из Avro схем
- Использовать сгенерированные классы с Confluent.SchemaRegistry.Serdes.Avro

### Предварительные требования

1. Запустите локальную инфраструктуру:
```powershell
cd ..\infrastructure
.\start.ps1
```

2. Соберите пример:
```powershell
dotnet build
```

Сгенерированные классы будут помещены в директорию `Generated`.
