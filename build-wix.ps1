param(
    [string]$Version = "1.0.0",
    [string]$PublishPath = ".\publish\win-x64",
    [string]$OutputPath = ".\BigFileHunter-x64.msi"
)

$ErrorActionPreference = "Stop"

Write-Host "Building BigFileHunter WiX installer v$Version" -ForegroundColor Cyan

# Generate file list wxs file using PowerShell (WiX v4 doesn't have heat.exe)
Write-Host "Generating file list..." -ForegroundColor Yellow

$publishPathResolved = Resolve-Path $PublishPath
$files = Get-ChildItem -Path $publishPathResolved -File | Where-Object { $_.Name -ne "BigFileHunter.exe" }

# Create the generated wxs file
$wxsPath = ".\BigFileHunter.Setup\FilesGenerated.wxs"
$writer = [System.IO.StreamWriter]::new($wxsPath, $false, [System.Text.Encoding]::UTF8)

$writer.WriteLine('<?xml version="1.0" encoding="UTF-8"?>')
$writer.WriteLine('<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">')
$writer.WriteLine('  <Fragment>')
$writer.WriteLine('    <ComponentGroup Id="GeneratedComponents" Directory="INSTALLFOLDER">')

$componentId = 1
foreach ($file in $files) {
    $fileId = "file_" + ($file.Name -replace '[^\w\d.]', '_')
    $fileName = $file.Name

    $writer.WriteLine("      <Component Id=`"GeneratedFile$componentId`" Guid=`"*`">")
    $writer.WriteLine("        <File Id=`"$fileId`" Source=`"`$(var.PublishPath)\$fileName`" />")
    $writer.WriteLine("      </Component>")

    $componentId++
}

$writer.WriteLine('    </ComponentGroup>')
$writer.WriteLine('  </Fragment>')
$writer.WriteLine('</Wix>')

$writer.Dispose()

Write-Host "Generated $wxsPath with $($files.Count) files" -ForegroundColor Green

# Build the MSI
Write-Host "Building MSI..." -ForegroundColor Yellow
wix build ".\BigFileHunter.Setup\Package.wxs" `
    ".\BigFileHunter.Setup\Files.wxs" `
    ".\BigFileHunter.Setup\FilesGenerated.wxs" `
    -arch x64 `
    -bindpath $PublishPath `
    -out $OutputPath `
    -d "Version=$Version" `
    -d "PublishPath=$PublishPath" `
    -ext "WixToolset.UI.wixext"

if ($LASTEXITCODE -ne 0) {
    Write-Host "wix build failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "Successfully built $OutputPath" -ForegroundColor Green
