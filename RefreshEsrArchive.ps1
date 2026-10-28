$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$runewords = 'https://d2esrdb.github.io/Eastern_Sun_Resurrected/runewords.htm'
$recipes = 'https://d2esrdb.github.io/Eastern_Sun_Resurrected/Eastern%20Sun%20Resurrected%20Cube%20Recipes.html'
Invoke-WebRequest -UseBasicParsing -Uri $runewords -OutFile 'esr-runewords.htm'
Invoke-WebRequest -UseBasicParsing -Uri $recipes -OutFile 'esr-cube-recipes.htm'
$csc=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
& $csc /nologo /target:exe '/out:BuildEsrArchive.exe' /reference:System.dll /reference:System.Core.dll /reference:System.Xml.dll 'BuildEsrArchive.cs'
if($LASTEXITCODE -ne 0){throw 'ESR archive tool build failed.'}
& .\BuildEsrArchive.exe 'esr-runewords.htm' 'esr-cube-recipes.htm' 'esr-archive.xml'
if($LASTEXITCODE -ne 0){throw 'ESR archive refresh failed.'}
Write-Host 'Updated esr-archive.xml from the ESR 2.31 public reference pages.'
