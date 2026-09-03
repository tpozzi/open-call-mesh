param([string]$InstallRoot = "$env:ProgramFiles\OpenCallMesh\Agent")
New-Item -ItemType Directory -Force -Path $InstallRoot | Out-Null
Write-Host "Build and publish the Agent to $InstallRoot before installing a service."
