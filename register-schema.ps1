$schemaContent = Get-Content -Path "schemas/test-schema.avsc" -Raw
$body = @{
    schema = $schemaContent
} | ConvertTo-Json -Depth 10 -Compress

$headers = @{
    "Content-Type" = "application/vnd.schemaregistry.v1+json"
}

Invoke-RestMethod -Method Post -Uri "http://localhost:8081/subjects/test-schema-value/versions" -Headers $headers -Body $body
