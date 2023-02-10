# Script builds a Chocolatey Package and tests it locally
# 
#  Assumes: Uses latest release out of Pre-release folder
#           Release has been checked in to GitHub Repo
#   Builds: ChocolateyInstall.ps1 file with download URL and sha256 embedded

Set-Location "$PSScriptRoot" 

copy ..\HtmlPackager.exe .\tools

$sha = get-filehash -path ".\tools\HtmlPackager.exe" -Algorithm SHA256  | select -ExpandProperty "Hash"
write-host $sha

$filetext = @"
`VERIFICATION
`HtmlPackager.zip
`SHA256 Checksum Value: $sha
"@
out-file -filepath .\tools\Verification.txt -inputobject $filetext


$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo("$PSScriptRoot\tools\HtmlPackager.exe").FileVersion
$version = $version.Trim()
"Initial Version: " + $version

# Remove last two .0 version tuples if it's 0
if($version.EndsWith(".0.0")) {
    $version = $version.SubString(0,$version.Length - 4);
}
else {
    if($version.EndsWith(".0")) {    
        $version = $version.SubString(0,$version.Length - 2);
    }
}
"Truncated Version: " + $version

$versionFilePath = ".\HtmlPackager.nuspec-template"
$versionFile = Get-Content -Path $versionFilePath  

$versionFile = $versionFile.Replace("{version}",$version)
$versionFile
""
out-file -filepath $versionFilePath.Replace("-Template","")  -inputobject $versionFile


# Create .nupkg from .nuspec
choco pack

choco uninstall "HtmlPackager" -f

choco install "HtmlPackager" -fd  -y -s ".\"

#choco push