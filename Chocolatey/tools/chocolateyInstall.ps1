﻿#NOTE: Please remove any commented lines to tidy up prior to releasing the package, including this one

$packageName = 'HtmlPackager' # arbitrary name for the package, used in messages
$url = 'https://github.com/RickStrahl/Westwind.HtmlPackager/raw/master/HtmlPackager.exe' # download url
$drop = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

# main helpers - these have error handling tucked into them already
# installer, will assert administrative rights

# if removing $url64, please remove from here
Install-ChocolateyZipPackage $packageName $url  $drop