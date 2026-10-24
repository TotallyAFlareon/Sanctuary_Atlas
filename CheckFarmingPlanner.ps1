$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path 'Sanctuary Atlas.exe').Path)
$planner = $assembly.GetType('FarmingPlanner')
$routes = $planner.GetField('Routes').GetValue($null)
if ($routes.Length -lt 15) { throw 'Farming planner routes are incomplete.' }
$find = $planner.GetMethod('Find')
foreach($act in 1..5){
 $matches=@($find.Invoke($null,@($act,'Any goal',3)))
 if($matches.Count -eq 0){throw "Act $act has no farming route."}
}
$safe=@($find.Invoke($null,@(0,'Experience',1)))
if($safe.Count -lt 2 -or @($safe | Where-Object { $_.Act -eq 1 }).Count -eq 0 -or @($safe | Where-Object { $_.Act -eq 2 }).Count -eq 0){throw 'Safer experience filters are missing expected routes.'}
$keys=@($find.Invoke($null,@(0,'Keys',3)))
if($keys.Count -ne 2 -or -not ($keys.Name -contains 'Summoner key pass') -or -not ($keys.Name -contains 'Nihlathak key run')){throw 'Key routes are incorrect.'}
$flags=[Reflection.BindingFlags]'Instance,NonPublic'
$dir=Join-Path (Get-Location) ('test-builds\\farming-' + [Guid]::NewGuid().ToString('N'))
$app=New-Object Atlas($dir)
try {
 $assembly.GetType('Atlas').GetMethod('ShowFarmingPlanner',$flags).Invoke($app,@()) | Out-Null
 $panel=$assembly.GetType('Atlas').GetField('content',$flags).GetValue($app)
 $list=@($panel.Controls | Where-Object { $_.AccessibleName -eq 'Farming planner routes' })[0]
 $details=@($panel.Controls | Where-Object { $_.AccessibleName -eq 'Farming planner details' })[0]
 if($null -eq $list -or $null -eq $details -or $list.Items.Count -eq 0){throw 'Farming planner UI did not render.'}
 $list.SelectedIndex=0
 if(-not $details.Text.Contains('START') -or -not $details.Text.Contains('SAFETY')){throw 'Route details are missing.'}
 $app.Size=$app.MinimumSize;$app.PerformLayout()
 $side=@($app.Controls | Where-Object { $_.Dock -eq [Windows.Forms.DockStyle]::Left })[0]
 $footer=@($side.Controls | Where-Object { $_.Dock -eq [Windows.Forms.DockStyle]::Bottom })[0]
 $nav=@($side.Controls | Where-Object { $_.Text -eq '11   FARMING PLANNER' })[0]
 if($null -eq $nav -or $nav.Bounds.IntersectsWith($footer.Bounds)){throw 'Farming planner navigation overlaps update controls at minimum size.'}
 Write-Output 'Farming planner routes, filters and navigation passed.'
} finally {$app.Dispose()}

