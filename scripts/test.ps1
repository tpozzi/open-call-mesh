$ErrorActionPreference = 'Stop'
dotnet run --project (Join-Path $PSScriptRoot '..\tests\OpenCallMesh.Core.Tests')
dotnet run --project (Join-Path $PSScriptRoot '..\tests\OpenCallMesh.Protocol.Tests')
dotnet run --project (Join-Path $PSScriptRoot '..\tests\OpenCallMesh.Transport.Tests')
