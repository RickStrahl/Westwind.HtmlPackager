$release = '..\HtmlPackager\bin\Release'

remove-item $release\*.pdb

$windir = $env:windir
$platform = "v4,$windir\Microsoft.NET\Framework64\v4.0.30319"

# Merge Dlls into single EXE
.\ilmerge /t:exe /targetplatform:$platform /lib:. /out:..\HtmlPackager.exe $release\HtmlPackager.exe $release\Westwind.HtmlPackager.dll $release\HtmlAgilityPack.dll..\

remove-item ..\HtmlPackager.pdb