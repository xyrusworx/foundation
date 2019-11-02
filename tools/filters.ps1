function Filter-Output {
    Param(
        [Parameter(ValueFromPipeline = $true)] [string] $Text,
        [Parameter(Mandatory = $false)] [string] $Scope = $null
    )
    Begin {
        $regex = New-Object "System.Text.RegularExpressions.Regex" -ArgumentList @('^##([a-z_][a-z0-9_]+)\[(.*?)\]$', ([Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [Text.RegularExpressions.RegexOptions]::Compiled))
    }
    Process {

        if ([string]::IsNullOrWhiteSpace($Text)) {
            return
        }

        $match = $regex.Match($Text)

        if (-not $match.Success) {
            return
        }

        if ([string]::IsNullOrWhiteSpace($Scope) -or ($match.Groups[1].Value -ieq $Scope)) {
            Write-Output $match.Groups[2].Value
        }
    }
}