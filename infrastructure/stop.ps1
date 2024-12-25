Write-Host "Stopping Kafka infrastructure..."
docker-compose -f docker-compose.yml down

Write-Host "Infrastructure stopped."
