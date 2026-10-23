$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Windows.Forms
[Reflection.Assembly]::LoadFrom((Resolve-Path 'Sanctuary Atlas.exe').Path) | Out-Null
$flags=[Reflection.BindingFlags]'Instance,NonPublic'
$type=[Atlas]
$folder=Join-Path (Get-Location) ('test-builds\library-'+[Guid]::NewGuid().ToString('N'))
$app=New-Object Atlas($folder)
try {
 $type.GetMethod('ShowItemLibrary',$flags).Invoke($app,@())
 $content=$type.GetField('content',$flags).GetValue($app)
 $list=@($content.Controls | Where-Object { $_.AccessibleName -eq 'Item library results' })[0]
 $query=@($content.Controls | Where-Object { $_.AccessibleName -eq 'Search item library' })[0]
 $kind=@($content.Controls | Where-Object { $_.AccessibleName -eq 'Item category' })[0]
 $info=@($content.Controls | Where-Object { $_.AccessibleName -eq 'Item library information' })[0]
 if($list.Items.Count -lt 100){throw 'Item library is missing entries.'}
 $query.Text='harlequin'
 if($list.Items.Count -ne 1 -or -not $info.Text.Contains('+2 to All Skills')){throw 'Unique search or tracked bonuses failed.'}
 $button=@($content.Controls | Where-Object { $_ -is [Windows.Forms.Button] -and $_.Text -eq 'EQUIP PRESET' })[0]
 [Windows.Forms.Control].GetMethod('OnClick',$flags).Invoke($button,@([EventArgs]::Empty))
 $build=$type.GetField('build',$flags).GetValue($app)
 if($build.Gear[2] -ne 'Harlequin Crest' -or $build.GearSkillBonuses[2].Lines -ne 'All Skills: +2'){throw 'Equipped item or skill bonus was not stored.'}
 $type.GetMethod('ShowItemLibrary',$flags).Invoke($app,@())
 $content=$type.GetField('content',$flags).GetValue($app)
 $list=@($content.Controls | Where-Object { $_.AccessibleName -eq 'Item library results' })[0]
 $query=@($content.Controls | Where-Object { $_.AccessibleName -eq 'Search item library' })[0]
 $kind=@($content.Controls | Where-Object { $_.AccessibleName -eq 'Item category' })[0]
 $info=@($content.Controls | Where-Object { $_.AccessibleName -eq 'Item library information' })[0]
 $kind.SelectedItem='Runewords';$query.Text='Spirit'
 if($list.Items.Count -lt 1 -or -not $info.Text.Contains('RUNE ORDER')){throw 'Runeword search or recipe failed.'}
 $query.Text='this item does not exist'
 if($list.Items.Count -ne 0 -or $info.Text -ne 'No matching items.'){throw 'Empty results failed.'}
 $app.Dispose()
 $again=New-Object Atlas($folder)
 try { $saved=$type.GetField('build',$flags).GetValue($again);if($saved.Gear[2] -ne 'Harlequin Crest' -or $saved.GearSkillBonuses[2].Lines -ne 'All Skills: +2'){throw 'Equipped preset did not persist.'} } finally {$again.Dispose()}
 Write-Output 'Item search, category filters, runeword details, preset equipment and persistence passed.'
} finally {if(-not $app.IsDisposed){$app.Dispose()}}

