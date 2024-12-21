# Build script for local development

# Clean previous builds
if (Test-Path "nupkg") {
    Remove-Item -Path "nupkg" -Recurse -Force
}

# Restore dependencies
dotnet restore

# Build solution
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release --no-build

# Create packages
dotnet pack src/AvroGen.NET/AvroGen.NET.csproj --configuration Release --no-build --output nupkg
dotnet pack src/AvroGen.NET.Tool/AvroGen.NET.Tool.csproj --configuration Release --no-build --output nupkg

Write-Host "NuGet packages created in ./nupkg directory"
Write-Host "To install the tool locally:"
Write-Host "dotnet tool install --global --add-source ./nupkg AvroGen.NET.Tool"
