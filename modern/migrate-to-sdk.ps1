<#
.SYNOPSIS
  Migrate old .NET Framework csproj to SDK-style .NET 8

.DESCRIPTION
  Converts ToolsVersion 4.0 csproj with explicit <Compile Include> to SDK-style
  with globbing, PackageReference, and modern TFMs. Keeps source files.

  This is similar to `try-convert` but tailored for Pulse's provider structure.

.PARAMETER Project
  Path to old csproj to convert

.PARAMETER TargetFramework
  Target framework, default net8.0-windows10.0.22621.0

.EXAMPLE
  .\migrate-to-sdk.ps1 -Project Pulse/Pulse.Base/Pulse.Base.csproj -TargetFramework net8.0-windows10.0.22621.0
#>
param(
    [Parameter(Mandatory=$true)][string]$Project,
    [string]$TargetFramework = "net8.0-windows10.0.22621.0",
    [switch]$UseWPF,
    [switch]$UseWindowsForms,
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $Project)) {
    Write-Error "Project not found: $Project"
    exit 1
}

[xml]$old = Get-Content $Project
$nsmgr = New-Object System.Xml.XmlNamespaceManager $old.NameTable
$nsmgr.AddNamespace("msb","http://schemas.microsoft.com/developer/msbuild/2003")

function Get-XmlText($xpath) {
    $node = $old.SelectSingleNode($xpath, $nsmgr)
    if ($node) { return $node.InnerText } else { return $null }
}

# Extract data with namespace handling
$assemblyName = Get-XmlText "//msb:AssemblyName"
if (-not $assemblyName) { $assemblyName = [IO.Path]::GetFileNameWithoutExtension($Project) }
$rootNamespace = Get-XmlText "//msb:RootNamespace"
if (-not $rootNamespace) { $rootNamespace = $assemblyName }
$outputType = Get-XmlText "//msb:OutputType"
if (-not $outputType) { $outputType = "Library" }

$isWpf = $Project -like "*Pulse.csproj" -or $UseWPF.IsPresent
$isWinForms = $UseWindowsForms.IsPresent -or ($Project -like "*Forms*" -or $Project -like "*Pulse*")
# Check for WPF Page includes via namespace-aware
$hasXaml = $old.SelectNodes("//msb:Page", $nsmgr).Count -gt 0
if ($hasXaml) { $isWpf = $true }

# Escape XML special chars for assembly name
function Escape-Xml($s) {
    return $s -replace "&","&amp;" -replace "<","&lt;" -replace ">","&gt;" -replace '"',"&quot;" -replace "'","&apos;"
}
$assemblyNameEsc = Escape-Xml $assemblyName
$rootNamespaceEsc = Escape-Xml $rootNamespace

# Generate new SDK csproj
$manifestFile = Join-Path (Join-Path (Split-Path $Project) "Properties") "app.manifest"
$hasManifest = Test-Path $manifestFile
$newCsproj = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>$outputType</OutputType>
    <TargetFramework>$TargetFramework</TargetFramework>
    $(if ($isWpf) { "<UseWPF>true</UseWPF>" })
    $(if ($isWinForms) { "<UseWindowsForms>true</UseWindowsForms>" })
    <AssemblyName>$assemblyNameEsc</AssemblyName>
    <RootNamespace>$rootNamespaceEsc</RootNamespace>
    <LangVersion>12</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <EnableWindowsTargeting>true</EnableWindowsTargeting>
    $(if ($hasManifest) { "<ApplicationManifest>Properties\app.manifest</ApplicationManifest>" })
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="System.Drawing.Common" Version="8.0.8" />
    <PackageReference Include="System.Security.Cryptography.ProtectedData" Version="8.0.0" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>

  <ItemGroup>
"@

# Project references with namespace handling
$projRefs = $old.SelectNodes("//msb:ProjectReference", $nsmgr)
foreach ($pr in $projRefs) {
    $inc = $pr.GetAttribute("Include")
    if ($inc) {
        $incEsc = Escape-Xml $inc
        $newCsproj += "    <ProjectReference Include=`"$incEsc`" />`n"
    }
}

# Handle packages.config
$pkgRefs = ""
$pkgConfigPath = Join-Path (Split-Path $Project) "packages.config"
if (Test-Path $pkgConfigPath) {
    try {
        [xml]$pkg = Get-Content $pkgConfigPath
        foreach ($p in $pkg.packages.package) {
            $id = $p.id
            $ver = $p.version
            if ($id -and $ver -and $id -ne "CsQuery") {
                $pkgRefs += "    <PackageReference Include=`"$id`" Version=`"$ver`" />`n"
            }
        }
    } catch {}
}
if ($pkgRefs) {
    $newCsproj += "  </ItemGroup>`n  <ItemGroup>`n$pkgRefs"
}

$newCsproj += @"
  </ItemGroup>
</Project>
"@

$backup = "$Project.old"
if (-not $DryRun) {
    Copy-Item $Project $backup -Force
    Write-Host "Backed up to $backup"
    # UTF8 no BOM for SDK-style
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($Project, $newCsproj, $utf8NoBom)
    Write-Host "Migrated $Project -> SDK style ($TargetFramework)"
    if ($isWpf) { Write-Host "  UseWPF=true (XAML detected)" }
    if ($isWinForms) { Write-Host "  UseWindowsForms=true" }
} else {
    Write-Host "DryRun: would generate:"
    Write-Host $newCsproj
}

# Check for packages.config
$pkgConfig = Join-Path (Split-Path $Project) "packages.config"
if (Test-Path $pkgConfig) {
    Write-Host "Found $pkgConfig - migrated to PackageReference where possible, removing (CsQuery already removed)"
    if (-not $DryRun) {
        # Keep backup
        Copy-Item $pkgConfig "$pkgConfig.old" -Force -ErrorAction SilentlyContinue
        Remove-Item $pkgConfig -Force -ErrorAction SilentlyContinue
        Write-Host "Removed $pkgConfig (backup .old)"
    }
}
