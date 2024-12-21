# Infrastructure

[Русская версия](README.ru.md)

This directory contains the infrastructure setup for running Kafka and Schema Registry locally using Docker.

## Prerequisites

- Docker
- Docker Compose
- PowerShell

## Components

- Apache Kafka (Confluent Platform 7.5.3)
- Schema Registry (Confluent Platform 7.5.3)
- ZooKeeper (Confluent Platform 7.5.3)
- Schema Registry UI

## Ports

- Kafka: 9092 (external), 29092 (internal)
- Schema Registry: 8081
- Schema Registry UI: 8000
- ZooKeeper: 2181

## Usage

### Starting the Infrastructure

```powershell
.\start.ps1
```

This will:
1. Start ZooKeeper
2. Start Kafka broker
3. Start Schema Registry
4. Start Schema Registry UI
5. Register test schema from ..\schemas\test-schema.avsc
6. Wait for all services to be ready

### Stopping the Infrastructure

```powershell
.\stop.ps1
```

This will stop and remove all containers.

### Registering a New Schema

To register a new schema manually:

```powershell
.\register-schema.ps1 -SchemaFile path\to\schema.avsc -Subject my-schema-value
```

Parameters:
- `SchemaFile`: Path to the Avro schema file
- `Subject`: Name of the subject in Schema Registry (typically `{topic-name}-value` or `{topic-name}-key`)
- `SchemaRegistryUrl`: (Optional) URL of the Schema Registry (default: http://localhost:8081)

## Verification

To verify that everything is working:

1. Schema Registry should be accessible at: http://localhost:8081
2. Schema Registry UI should be accessible at: http://localhost:8000
3. Kafka broker should be accessible at: localhost:9092

## Troubleshooting

If you encounter any issues:

1. Check if Docker is running
2. Check if ports are not in use by other applications
3. Try stopping and starting the infrastructure again
4. Check Docker logs:
   ```powershell
   docker-compose logs
   ```

### Common Issues

1. Schema registration fails:
   - Check if Schema Registry is running: `docker ps | grep schema-registry`
   - Check Schema Registry logs: `docker-compose logs schema-registry`
   - Verify schema file exists and is valid JSON
   - Check if Schema Registry is accessible: `curl http://localhost:8081/subjects`
