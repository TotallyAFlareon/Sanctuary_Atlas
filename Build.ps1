$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$csc=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
& $csc /nologo /target:winexe '/out:Sanctuary Atlas.exe' '/win32icon:sanctuary.ico' /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Xml.dll /reference:System.Core.dll /reference:System.Web.Extensions.dll '/resource:sanctuary-background.png,SanctuaryBackground' '/resource:runeword-effects.xml,RunewordEffects' 'Atlas.cs' 'Updater.cs' 'SkillDetailView.cs' 'SkillDetailWindow.cs' 'SkillCalculations.cs' 'SkillTooltipRenderer.cs' 'PreviewTooltipSession.cs' 'GearSkillBonuses.cs' 'GuideBook.cs' 'GuideDetails.cs' 'Breakpoints.cs' 'CuratedBuilds.cs'
if($LASTEXITCODE -ne 0){throw 'Application build failed.'}
& $csc /nologo /target:winexe '/out:SanctuaryAtlasSetup.exe' '/win32icon:sanctuary.ico' /reference:System.Windows.Forms.dll /reference:System.Drawing.dll '/resource:Sanctuary Atlas.exe,AtlasApp' '/resource:runewizard-LICENSE.txt,RunewizardLicense' 'Setup.cs'
if($LASTEXITCODE -ne 0){throw 'Installer build failed.'}
Write-Host 'Built SanctuaryAtlasSetup.exe'

