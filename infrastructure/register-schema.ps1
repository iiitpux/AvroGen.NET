param(
    [Parameter(Mandatory=$true)]
    [string]$SchemaFile,
    
    [Parameter(Mandatory=$true)]
    [string]$Subject,
    
    [string]$SchemaRegistryUrl = "http://localhost:8081"
)

# Check if schema file exists
if (-not (Test-Path $SchemaFile)) {
    Write-Error "Schema file not found: $SchemaFile"
    exit 1
}

# Read schema content
$schemaContent = [System.IO.File]::ReadAllText($SchemaFile)

# Create request body with schema as string
$body = @{
    schema = $schemaContent
} | ConvertTo-Json -Compress

Write-Host "Request body: $body"

Write-Host "Registering schema for subject: $Subject"
Write-Host "Schema Registry URL: $SchemaRegistryUrl"

try {
    # Register schema
    $response = Invoke-RestMethod -Method POST `
        -Uri "$SchemaRegistryUrl/subjects/$Subject/versions" `
        -ContentType "application/json" `
        -Body $body

    Write-Host "Schema registered successfully!"
    Write-Host "Schema ID: $($response.id)"
}
catch {
    Write-Error "Failed to register schema: $_"
    exit 1
}
