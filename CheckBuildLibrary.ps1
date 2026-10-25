$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path 'Sanctuary Atlas.exe').Path)
$flags = [Reflection.BindingFlags]'Instance,NonPublic'
$atlasType = $assembly.GetType('Atlas')
$dir = Join-Path (Get-Location) ('test-builds\library-' + [Guid]::NewGuid().ToString('N'))
$app = New-Object Atlas($dir)
try {
 $build = $atlasType.GetField('build',$flags).GetValue($app)
 $build.Level = 48
 $build.Quests = 12
 $build.Points[0] = 4
 $build.Gear[0] = 'Spirit'
 $build.Runes[0] = 3
 $build.Notes = 'Keep this plan'
 $save = $atlasType.GetMethod('SaveSnapshot',$flags)
 $save.Invoke($app,@('Demon test')) | Out-Null
 $snapshots = $atlasType.GetField('snapshots',$flags).GetValue($app)
 if($snapshots.Count -ne 1){throw 'Build snapshot was not saved.'}
 $snapshot = $snapshots[0]
 if($snapshot.ClassName -ne 'Warlock' -or $snapshot.Data.Level -ne 48 -or $snapshot.Data.Gear[0] -ne 'Spirit' -or $snapshot.Data.Notes -ne 'Keep this plan'){throw 'Snapshot did not preserve current build data.'}
 $build.Level = 10
 $build.Gear[0] = 'Changed'
 $save.Invoke($app,@('Demon test')) | Out-Null
 if($snapshots.Count -ne 1 -or $snapshots[0].Data.Level -ne 10 -or $snapshots[0].Data.Gear[0] -ne 'Changed'){throw 'Matching snapshot name did not replace saved copy.'}
 if(-not (Test-Path (Join-Path $dir 'build-library.xml'))){throw 'Build library file was not saved.'}
 $atlasType.GetMethod('ShowBuildLibrary',$flags).Invoke($app,@()) | Out-Null
 $panel = $atlasType.GetField('content',$flags).GetValue($app)
 $list = @($panel.Controls | Where-Object { $_.AccessibleName -eq 'Build library snapshots' })[0]
 $details = @($panel.Controls | Where-Object { $_.AccessibleName -eq 'Build snapshot details' })[0]
 if($null -eq $list -or $list.Items.Count -ne 1 -or -not $details.Text.Contains('Demon test') -or -not $details.Text.Contains('Changed')){throw 'Build library UI did not show saved snapshot.'}
 $app.Size = $app.MinimumSize
 $app.PerformLayout()
 $side = @($app.Controls | Where-Object { $_.Dock -eq [Windows.Forms.DockStyle]::Left })[0]
 $footer = @($side.Controls | Where-Object { $_.Dock -eq [Windows.Forms.DockStyle]::Bottom })[0]
 $nav = @($side.Controls | Where-Object { $_.Text -eq '12   BUILD LIBRARY' })[0]
 if($null -eq $nav -or $nav.Bounds.IntersectsWith($footer.Bounds)){throw 'Build library navigation overlaps update controls at minimum size.'}
 $app.Dispose()
 $reopened = New-Object Atlas($dir)
 try {
  $atlasType.GetMethod('ShowBuildLibrary',$flags).Invoke($reopened,@()) | Out-Null
  $panel = $atlasType.GetField('content',$flags).GetValue($reopened)
  $list = @($panel.Controls | Where-Object { $_.AccessibleName -eq 'Build library snapshots' })[0]
  if($list.Items.Count -ne 1 -or $list.Items[0].Data.Level -ne 10){throw 'Saved build library was not restored.'}
 } finally {$reopened.Dispose()}
 Write-Output 'Build library save, replacement, persistence and navigation passed.'
} finally {
 if(-not $app.IsDisposed){$app.Dispose()}
 if(Test-Path $dir){Remove-Item -LiteralPath $dir -Recurse -Force}
}
