$ErrorActionPreference = 'Stop'
dotnet build (Join-Path $PSScriptRoot '..\OpenCallMesh.sln')
