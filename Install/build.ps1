Clear-Host
Set-Location "$PSScriptRoot" 
$release = "$PSScriptRoot\..\HtmlPackagerConsole\bin\Release\net472"
Write-Host $release

remove-item $release\*.pdb

$windir = $env:windir
$platform = "v4,$windir\Microsoft.NET\Framework64\v4.0.30319"

$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo("$release\HtmlPackager.exe").FileVersion
$version = $version.Trim()
"Initial Version: " + $version


# Merge Dlls into single EXE
.\ilmerge /t:exe /ver:$version /targetplatform:$platform /lib:. /out:..\HtmlPackager.exe $release\HtmlPackager.exe $release\Westwind.HtmlPackager.dll $release\HtmlAgilityPack.dll

& ".\signtool.exe" sign /v /n "West Wind Technologies"  /tr "http://timestamp.digicert.com" /td SHA256 /fd SHA256 "..\HtmlPackager.exe"

remove-item ..\HtmlPackager.pdb