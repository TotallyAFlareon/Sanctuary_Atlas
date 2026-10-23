$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms
[Reflection.Assembly]::LoadFrom((Resolve-Path 'Sanctuary Atlas.exe').Path) | Out-Null
$build=New-Object Build
$build.Gear[0]='Wand';$build.GearSkillBonuses[0]=New-Object ItemSkillBonuses
$build.GearSkillBonuses[0].Fcr=20
$build.GearSkillBonuses[0].OtherStats.Add((New-Object ItemStat -Property @{Name='Fire Resistance (%)';Value=15}))
$build.Inventory[0]='Charm';$build.InventorySkillBonuses[0]=New-Object ItemSkillBonuses
$build.InventorySkillBonuses[0].ActiveCharm=$true
$build.InventorySkillBonuses[0].OtherStats.Add((New-Object ItemStat -Property @{Name='Fire Resistance (%)';Value=5}))
$build.Inventory[1]='Inactive';$build.InventorySkillBonuses[1]=New-Object ItemSkillBonuses
$build.InventorySkillBonuses[1].Fcr=50
$build.InventorySkillBonuses[1].OtherStats.Add((New-Object ItemStat -Property @{Name='Fire Resistance (%)';Value=50}))
$totals=[GearOverview]::ForBuild($build)
if($totals['Fire Resistance (%)'] -ne 20){throw 'Equipment or active charm totals incorrect.'}
$skills=New-Object 'System.Collections.Generic.List[Skill]'
$summary=[GearOverview]::Format($build,$skills,$false)
if(-not $summary.Contains('Faster Cast Rate: 20%') -or -not $summary.Contains('Fire Resistance (%): +20')){throw 'Overview totals incorrect.'}
if(-not $summary.Contains('not final character values')){throw 'Overview needs its calculation scope.'}
Write-Output 'Gear totals, active charm handling and calculation scope passed.'

