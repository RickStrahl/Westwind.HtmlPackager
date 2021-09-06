$signingCertificateSubject = "West Wind Technologies"

if (test-path ./nupkg) {
    remove-item ./nupkg -Force -Recurse
} 

dotnet build -c Release


# find latest .nupkg file
$filename = gci "./nupkg" | sort LastWriteTime | select -last 1 | select -ExpandProperty "Name"
Write-host $filename

$len = $filename.length
if ($len -gt 0) {
    Write-Host "signing..."
    nuget sign  ".\nupkg\$filename"   -CertificateSubject $signingCertificateSubject -timestamper "http://timestamp.comodoca.com"
    #nuget push  ".\nupkg\$filename" -source nuget.org
}

# Install once it shows up - about 5 minutes or so
# dotnet tool update -g dotnet-htmlpackager