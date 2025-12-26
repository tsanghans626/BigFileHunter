# Generate appcast.xml
param(
    [string]$Version,
    [string]$ReleaseDate,
    [hashtable]$Signatures,
    [string]$ArtifactsPath
)

$osMap = @{
    "x64" = "windows-x64"
    "x86" = "windows-x86"
    "arm64" = "windows-arm64"
}

$appcastContent = @"
<?xml version="1.0" encoding="UTF-8"?>
<rss xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:sparkle="http://www.andymatuschak.org/xml-namespaces/sparkle" version="2.0">
    <channel>
        <title>BigFileHunter</title>
        <link>https://tsanghans626.github.io/BigFileHunter/updates/appcast.xml</link>
        <description>BigFileHunter - Scan directories to identify disk usage patterns</description>
        <language>en</language>
"@

$architectures = @("x64", "x86", "arm64")

foreach ($arch in $architectures) {
    $msi = "$ArtifactsPath\BigFileHunter-$Version-$arch.msi"
    if (Test-Path $msi) {
        $size = (Get-Item $msi).Length
        $signature = $Signatures[$arch]

        $itemXml = @"
        <item>
            <title>Version $Version ($arch)</title>
            <sparkle:releaseNotesLink>https://github.com/tsanghans626/BigFileHunter/releases/tag/v$Version</sparkle:releaseNotesLink>
            <pubDate>$ReleaseDate</pubDate>
            <enclosure url="https://github.com/tsanghans626/BigFileHunter/releases/download/v$Version/BigFileHunter-$Version-$arch.msi"
                       sparkle:version="$Version"
                       sparkle:os="$($osMap[$arch])"
                       length="$size"
                       type="application/octet-stream"
                       sparkle:signature="$signature" />
        </item>
"@

        $appcastContent += $itemXml
    }
}

$appcastContent += @"
    </channel>
</rss>
"@

$outputPath = "$ArtifactsPath\appcast.xml"
# Use UTF-8 without BOM and LF line endings to match GitHub Pages deployed file
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
# Convert CRLF to LF for Unix-style line endings
$appcastContentLf = $appcastContent -replace "`r`n", "`n"
[System.IO.File]::WriteAllText($outputPath, $appcastContentLf, $utf8NoBom)
Write-Host "Generated appcast.xml at $outputPath"
