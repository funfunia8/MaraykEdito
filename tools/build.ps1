$ErrorActionPreference = "Stop"
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
