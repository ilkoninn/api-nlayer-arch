# App.API qovluğunda: .\ef.ps1 update | .\ef.ps1 add -Name Name
param(
	[Parameter(Position = 0)]
	[ValidateSet("update", "add")]
	[string]$Command = "update",
	[string]$Name = ""
)
$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot
dotnet tool restore | Out-Null
$dal = Join-Path $PSScriptRoot "..\App.DAL\App.DAL.csproj"
$startup = Join-Path $PSScriptRoot "App.API.csproj"
if ($Command -eq "update") {
	dotnet ef database update --project $dal --startup-project $startup
}
else {
	if (-not $Name) { throw "add üçün -Name lazımdır" }
	dotnet ef migrations add $Name --project $dal --startup-project $startup
}
