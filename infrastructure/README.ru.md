# Инфраструктура

[English version](README.md)

Этот каталог содержит инфраструктурные настройки для локального запуска Kafka и Schema Registry с использованием Docker.

## Предварительные требования

- Docker
- Docker Compose
- PowerShell

## Компоненты

- Apache Kafka (Confluent Platform 7.5.3)
- Schema Registry (Confluent Platform 7.5.3)
- ZooKeeper (Confluent Platform 7.5.3)
- Schema Registry UI

## Порты

- Kafka: 9092 (внешний), 29092 (внутренний)
- Schema Registry: 8081
- Schema Registry UI: 8000
- ZooKeeper: 2181

## Использование

### Запуск инфраструктуры

```powershell
.\start.ps1
```

Это выполнит:
1. Запуск ZooKeeper
2. Запуск Kafka брокера
3. Запуск Schema Registry
4. Запуск Schema Registry UI
5. Регистрацию тестовой схемы из ..\schemas\test-schema.avsc
6. Ожидание готовности всех сервисов

### Остановка инфраструктуры

```powershell
.\stop.ps1
```

Это остановит и удалит все контейнеры.

### Регистрация новой схемы

Для ручной регистрации новой схемы:

```powershell
.\register-schema.ps1 -SchemaFile path\to\schema.avsc -Subject my-schema-value
```

Параметры:
- `SchemaFile`: Путь к файлу схемы Avro
- `Subject`: Имя субъекта в Schema Registry (обычно `{topic-name}-value` или `{topic-name}-key`)
- `SchemaRegistryUrl`: (Опционально) URL Schema Registry (по умолчанию: http://localhost:8081)

## Проверка

Для проверки работоспособности:

1. Schema Registry должен быть доступен по адресу: http://localhost:8081
2. Schema Registry UI должен быть доступен по адресу: http://localhost:8000
3. Kafka брокер должен быть доступен по адресу: localhost:9092

## Устранение неполадок

Если возникли проблемы:

1. Проверьте, запущен ли Docker
2. Проверьте, не заняты ли порты другими приложениями
3. Попробуйте остановить и снова запустить инфраструктуру
4. Проверьте логи Docker:
   ```powershell
   docker-compose logs
   ```

### Частые проблемы

1. Не удается зарегистрировать схему:
   - Проверьте, запущен ли Schema Registry: `docker ps | grep schema-registry`
   - Проверьте логи Schema Registry: `docker-compose logs schema-registry`
   - Убедитесь, что файл схемы существует и является валидным JSON
   - Проверьте доступность Schema Registry: `curl http://localhost:8081/subjects`
