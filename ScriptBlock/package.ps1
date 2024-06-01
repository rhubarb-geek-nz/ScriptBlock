#!/usr/bin/env pwsh
# Copyright (c) 2023 Roger Brown.
# Licensed under the MIT License.

param($Configuration,$TargetFramework,$Platform,$IntDir,$OutDir,$PublishDir)

$ProjectName = 'ScriptBlock'
$CompanyName = 'rhubarb-geek-nz'

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$DSC = [System.IO.Path]::DirectorySeparatorChar

trap
{
	throw $PSItem
}

$xmlDoc = [System.Xml.XmlDocument](Get-Content "$ProjectName.csproj")

$ModuleId = $xmlDoc.SelectSingleNode("/Project/PropertyGroup/PackageId").FirstChild.Value
$Version = $xmlDoc.SelectSingleNode("/Project/PropertyGroup/Version").FirstChild.Value
$ProjectUri = $xmlDoc.SelectSingleNode("/Project/PropertyGroup/PackageProjectUrl").FirstChild.Value
$Description = $xmlDoc.SelectSingleNode("/Project/PropertyGroup/Description").FirstChild.Value
$Author = $xmlDoc.SelectSingleNode("/Project/PropertyGroup/Authors").FirstChild.Value
$Copyright = $xmlDoc.SelectSingleNode("/Project/PropertyGroup/Copyright").FirstChild.Value
$AssemblyName = $xmlDoc.SelectSingleNode("/Project/PropertyGroup/AssemblyName").FirstChild.Value

$CmdletsToExport = @("New-$ProjectName","Invoke-$ProjectName")

New-ModuleManifest -Path "$OutDir/$ModuleId.psd1" `
				-RootModule "$AssemblyName.dll" `
				-ModuleVersion $Version `
				-Guid '585cdf18-b083-4d2c-8933-ae3ca24186b5' `
				-Author $Author `
				-CompanyName $CompanyName `
				-Copyright $Copyright `
				-Description $Description `
				-FunctionsToExport @() `
				-CmdletsToExport $CmdletsToExport `
				-VariablesToExport '*' `
				-AliasesToExport @() `
				-ProjectUri $ProjectUri

Import-PowerShellDataFile -LiteralPath "$OutDir/$ModuleId.psd1" | Export-PowerShellDataFile | Set-Content -LiteralPath "$PublishDir$ModuleId.psd1" -Encoding utf8BOM

Remove-Item "$OutDir/$ModuleId.psd1"
