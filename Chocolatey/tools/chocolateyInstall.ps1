$packageName = 'HtmlPackager'
$url = 'https://github.com/RickStrahl/Westwind.HtmlPackager/raw/v0.1.2/HtmlPackager.exe'
$drop = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

Install-ChocolateyZipPackage $packageName $url $drop