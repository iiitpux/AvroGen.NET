@echo off
curl -X PUT -H "Content-Type: application/vnd.schemaregistry.v1+json" ^
  --data "{\"compatibility\": \"NONE\"}" ^
  http://localhost:8081/config/test-schema-value
