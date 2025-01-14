

Write-Host "Registering test schema..."
$schemaPath = Join-Path $PSScriptRoot "..\schemas\test-schema.avsc"
& $PSScriptRoot\register-schema.ps1 -SchemaFile $schemaPath -Subject "test-schema-value"

Write-Host "Registering task schema..."
$schemaPath = Join-Path $PSScriptRoot "..\schemas\task-schema.avsc"
& $PSScriptRoot\register-schema.ps1 -SchemaFile $schemaPath -Subject "task-schema-value"

Write-Host "Infrastructure is ready!"
Write-Host "Kafka: localhost:9092"
Write-Host "Schema Registry: http://localhost:8081"
Write-Host "Schema Registry UI: http://localhost:8000"
