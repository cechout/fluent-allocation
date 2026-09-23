# builds Setup\Output\Kurszuteilung_Installer.exe for the WPF version 1
# publishes the app and compiles KurszuteilungWPF_Setup.iss with the version read from the csproj, so the csproj stays
# the only place a version is written and the installer can never disagree with the app

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'Kurszuteilung\Kurszuteilung.csproj'

$xml = [xml](Get-Content $project)
$version = $xml.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
if (-not $version) { throw "no <Version> in $project" }

# self-contained, so the target machine needs no .NET runtime; Excel and LocalDB it still needs, and the installer
# warns when either is missing
# the output lands in bin\Release\net8.0-windows\win-x64\publish, which is where the .iss picks it up
dotnet publish $project -c Release -r win-x64 --self-contained true
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed' }

$iscc = Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'
if (-not (Test-Path $iscc)) { throw "Inno Setup 6 not found at $iscc" }

& $iscc "/DMyAppVersion=$version" (Join-Path $PSScriptRoot 'KurszuteilungWPF_Setup.iss')
if ($LASTEXITCODE -ne 0) { throw 'installer compile failed' }

Write-Host "Kurszuteilung v$version installer: $(Join-Path $PSScriptRoot 'Output\Kurszuteilung_Installer.exe')"
