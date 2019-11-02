function global:Build-Packages {
	param(
		$Scope = $null,
		$Target = "pack"
	);
	
	if ([string]::IsNullOrWhiteSpace($Scope)) {
		$Scope = @((gci -path "$PSScriptRoot\..\src" -directory -Exclude @("packages", "_ReSharper.Caches") | % { $_.Name }))
	}
	
	foreach($item in @($Scope)) {
		Write-Host -ForegroundColor Cyan "Building ""$item"" for target ""$Target"""
		Push-Location "$PSScriptRoot\cake" 
		&"$PSScriptRoot\cake\build.ps1" -Target "$Target" -Verbosity verbose -ScriptArgs "-scope=""$item"""
		Pop-Location
	}
}

function global:Create-Packages { 

	param(
		[Parameter(ValueFromPipeline = $true)] $Scope = $null
	)

	Process {
		Build-Packages -Scope $Scope -Target "Pack"
	}
}

function global:Publish-Packages { 

	param(
		[Parameter(ValueFromPipeline = $true)] $Scope = $null
	)

	Process {
		Build-Packages -Scope $Scope -Target "Publish"
	}
}

set-alias "build" "Create-Packages"
set-alias "publish" "Publish-Packages"