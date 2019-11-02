."$PSScriptRoot\filters.ps1"

function prompt
{
    $pl = (gl).Path
    $pb = [IO.Path]::GetFullPath("$PSScriptRoot\..")
    
    if ($pl.StartsWith($pb)) {
        $pl = $pl.Substring($pb.Length).TrimStart("\")
    }

    Write-Host ("$pl `$".Trim()) -nonewline -foregroundcolor White
    return " "
}

write-host -ForegroundColor cyan -NoNewline "PACMAN"
write-host -ForegroundColor white " Developer Shell"
write-host -ForegroundColor white "Copyright (c) XyrusWorx"

write-host -ForegroundColor Gray @"

Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal 
in the Software without restriction, including without limitation the rights 
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is fur-
nished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in 
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIA-
BILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
THE SOFTWARE.

"@

Push-Location "$PSScriptRoot\cake"
$SolutionName = ((&"./build.ps1" -target "init") | Filter-Output -Scope solutionName)
$Host.UI.RawUI.WindowTitle = "PACMAN Developer Shell - $SolutionName [" + ([IO.DirectoryInfo]([IO.Path]::GetFullPath("$PSScriptRoot\..\"))).Name + "]"
Pop-Location

function Reset-Environment {
    gci "$PSScriptRoot" -filter '*.ps1' | %{ . ($_.FullName) }
}

set-alias -name "reboot" -Value "Reset-Environment"