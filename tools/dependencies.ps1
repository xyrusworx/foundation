add-type -path "$PSScriptRoot\cake\XyrusWorx.Management.dll"

function Get-LocalFeedUrls {
    ([xml](gc "$PSScriptRoot\..\src\nuget.config")).configuration.packageSources.add `
        | ?{ $_.Success } `
        | %{$_.Groups[0].Value }
}

function Get-AvailablePackages {
    param(
        [Parameter(ValueFromPipeline = $true, Position = 0)] [string] $FeedUrl
    )

    process {
        $dict = New-Object "Collections.Generic.Dictionary[String,PSObject]"

        if ([string]::IsNullOrWhiteSpace($FeedUrl)) {
            return $dict
        }

        nuget list -source $FeedUrl | %{ 
            $t = $_.Split(' '); 
            $o = New-Object "PSObject" -Property @{ 
                Id = $t[0]
                Version = ([XyrusWorx.Management.SemanticVersion]::Parse($t[1])) 
            } 

            if (-not $dict.ContainsKey($t[0])) {
                $null = $dict.Add($t[0], $o)
            }
        }

        return $dict
    }
}

function Get-InstalledPackages {
    $dict = New-Object "Collections.Generic.Dictionary[String,PSObject]"

    gci -Path "$PSScriptRoot\..\src" -recurse -filter "*.csproj" | %{ 
        $xml = ([xml](gc $_.FullName)).Project.ItemGroup.PackageReference
        $file = $_

        @($xml) | % {
            $id = $_.Include
            $version = $_.Version

            if (-not [string]::IsNullOrWhiteSpace($id) -and -not [string]::IsNullOrWhiteSpace($version)) {
                if (-not $dict.ContainsKey($id)) {
                    $o = New-Object "PSObject" -Property @{ 
                        Id = $id
                        Version = ([XyrusWorx.Management.SemanticVersion]::Parse($version)) 
                        ProjectFiles = New-Object "Collections.Generic.List[PSObject]"
                        UsageCount = 1
                    } 

                    $null = $o.ProjectFiles.Add($file)
                    $null = $dict.Add($id, $o)
                }
                else {
                    $dict[$id].ProjectFiles.Add($file)
                    $dict[$id].UsageCount ++
                }
            }
        }
    }

    return $dict
}

function Update-Package {

    [CmdLetBinding(SupportsShouldProcess)]
    param(
        [Parameter(ValueFromPipeline = $true, Position = 0, Mandatory = $true)] $ProjectFile,
        [Parameter(ValueFromPipeline = $false, Position = 1, Mandatory = $true)] $Id,
        [Parameter(ValueFromPipeline = $false, Position = 2, Mandatory = $true)] $TargetVersion
    )

    process {
        if ($ProjectFile -is "String") {
            $ProjectFile = [IO.FileInfo] $ProjectFile
        }
        elseif ($ProjectFile -isnot "IO.FileInfo") {
            Write-Error -Message "File not found: $ProjectFile"
            return;
        }

        $doc = ([xml](gc $_.FullName))
        $xml = $doc.Project.ItemGroup.PackageReference
        $ver = ([XyrusWorx.Management.SemanticVersion]::Parse($TargetVersion)) 

        @($xml) | % {
            $currentId = "$($_.Include)".ToLowerInvariant()

            if ($currentId.Equals("$Id".ToLowerInvariant())) {
                if ($WhatIfPreference) {
                    Write-Host -ForegroundColor Cyan "SIMULATING: " -NoNewline
                }
                Write-Host "$($ProjectFile.BaseName): $Id -> $ver"
                $_.Version = $ver.ToString()
            }
        }

        if (-not $WhatIfPreference) {
            $doc.Save($ProjectFile.FullName)
            &"msbuild" /v:quiet /nologo /t:restore "$($ProjectFile.FullName)"
        }
    }
}

function Update-Packages {

    [CmdLetBinding(SupportsShouldProcess)]
    param(
        [Parameter(Mandatory = $false)] $Include,
        [Parameter(Mandatory = $false)] $Exclude
    )

    $feeds = @(Get-LocalFeedUrls | Get-AvailablePackages)

    $i = @($Include) | % { "$_".ToLowerInvariant() }
    $e = @($Exclude) | % { "$_".ToLowerInvariant() }

    (Get-InstalledPackages).Values | Sort-Object -Property "UsageCount" -Descending | % {
        $isIncluded = ($i.Length -eq 0) -or $i.Contains("$($_.Id)".ToLowerInvariant())
        $isExcluded = $e.Contains("$($_.Id)".ToLowerInvariant())

        if ($isIncluded -and -not $isExcluded) {

            $localPackage = $_
            $remotePackage = $null

            for($x = 0; $x -lt $feeds.Count; $x++) {
                if ($feeds[$x].ContainsKey($localPackage.Id)) {
                    $remotePackage = $feeds[$x][$localPackage.Id]
                    break;
                }
            }

            if ($remotePackage -ne $null) {
                $sourceVersion = $localPackage.Version
                $targetVersion = $remotePackage.Version

                if ($sourceVersion -lt $targetVersion) {
                    $localPackage.ProjectFiles | Update-Package -Id $localPackage.Id -TargetVersion "$targetVersion" -WhatIf:$WhatIfPreference
                }
                else {
                    Write-Verbose "$($localPackage.Id) is up-to-date ($sourceVersion)"
                }
            }
        }
    }
}

set-alias "migrate" "Update-Packages"