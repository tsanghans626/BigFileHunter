# Generate appcast.xml files (both combined and architecture-specific)
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

# Helper function to generate appcast header
function Get-AppcastHeader {
    return @"
<?xml version="1.0" encoding="UTF-8"?>
<rss xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:sparkle="http://www.andymatuschak.org/xml-namespaces/sparkle" version="2.0">
    <channel>
        <title>BigFileHunter</title>
        <link>https://tsanghans626.github.io/BigFileHunter/updates/appcast.xml</link>
        <description>BigFileHunter - Scan directories to identify disk usage patterns</description>
        <language>en</language>
"@
}

# Helper function to generate appcast footer
function Get-AppcastFooter {
    return @"
    </channel>
</rss>
"@
}

# Helper function to generate item XML for a specific architecture
function Get-ItemXml {
    param($arch, $version, $artifactsPath, $signature)

    $msi = "$ArtifactsPath\BigFileHunter-$Version-$arch.msi"
    if (-not (Test-Path $msi)) {
        return $null
    }

    $size = (Get-Item $msi).Length
    $osIdentifier = $osMap[$arch]

    return @"

        <item>
            <title>Version $version ($arch)</title>
            <link>https://github.com/tsanghans626/BigFileHunter/releases/tag/v$version</link>
            <description><![CDATA[
                <h2>What's New in BigFileHunter $version</h2>
                <p>Thank you for using BigFileHunter!</p>
                <h3>Recent Changes</h3>
                <ul>
                    <li>Check the <a href="https://github.com/tsanghans626/BigFileHunter/releases/tag/v$version">GitHub Release</a> for detailed change notes</li>
                </ul>
            ]]></description>
            <sparkle:releaseNotesLink>https://github.com/tsanghans626/BigFileHunter/releases/tag/v$version</sparkle:releaseNotesLink>
            <pubDate>$ReleaseDate</pubDate>
            <enclosure url="https://github.com/tsanghans626/BigFileHunter/releases/download/v$version/BigFileHunter-$version-$arch.msi"
                       sparkle:version="$version"
                       sparkle:os="$osIdentifier"
                       length="$size"
                       type="application/octet-stream"
                       sparkle:signature="$signature" />
        </item>
"@
}

$architectures = @("x64", "x86", "arm64")
$utf8NoBom = New-Object System.Text.UTF8Encoding $false

# Generate combined appcast.xml (for backward compatibility with v1.0.1 and earlier)
$appcastContent = Get-AppcastHeader
foreach ($arch in $architectures) {
    if ($Signatures.ContainsKey($arch)) {
        $itemXml = Get-ItemXml -arch $arch -version $Version -artifactsPath $ArtifactsPath -signature $Signatures[$arch]
        if ($itemXml) {
            $appcastContent += $itemXml
        }
    }
}
$appcastContent += Get-AppcastFooter

$combinedOutputPath = "$ArtifactsPath\appcast.xml"
$appcastContentLf = $appcastContent -replace "`r`n", "`n"
[System.IO.File]::WriteAllText($combinedOutputPath, $appcastContentLf, $utf8NoBom)
Write-Host "Generated combined appcast.xml at $combinedOutputPath"

# Generate architecture-specific appcast files
foreach ($arch in $architectures) {
    if ($Signatures.ContainsKey($arch)) {
        $archAppcastContent = Get-AppcastHeader
        $itemXml = Get-ItemXml -arch $arch -version $Version -artifactsPath $ArtifactsPath -signature $Signatures[$arch]
        if ($itemXml) {
            $archAppcastContent += $itemXml
            $archAppcastContent += Get-AppcastFooter

            $osIdentifier = $osMap[$arch]
            $archOutputPath = "$ArtifactsPath\appcast-$osIdentifier.xml"
            $archAppcastContentLf = $archAppcastContent -replace "`r`n", "`n"
            [System.IO.File]::WriteAllText($archOutputPath, $archAppcastContentLf, $utf8NoBom)
            Write-Host "Generated architecture-specific $archOutputPath"
        }
    }
}

Write-Host "All appcast files generated successfully"
