$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path 'Sanctuary Atlas.exe').Path)
$flags = [Reflection.BindingFlags]'Instance,NonPublic'
$atlasType = $assembly.GetType('Atlas')
$dir = Join-Path (Get-Location) ('test-builds\esr-' + [Guid]::NewGuid().ToString('N'))
$app = New-Object Atlas($dir)
try {
 $archive = $atlasType.GetField('esrArchive',$flags).GetValue($app)
 if($archive.Runewords.Count -lt 900 -or $archive.Recipes.Count -lt 500){throw 'The ESR archive did not load its full reference data.'}
 $atlasType.GetMethod('ShowEasternSunArchive',$flags).Invoke($app,@()) | Out-Null
 $panel = $atlasType.GetField('content',$flags).GetValue($app)
 $list = @($panel.Controls | Where-Object { $_.AccessibleName -eq 'ESR archive entries' })[0]
 $details = @($panel.Controls | Where-Object { $_.AccessibleName -eq 'ESR archive details' })[0]
 $search = @($panel.Controls | Where-Object { $_.AccessibleName -eq 'ESR archive search' })[0]
 if($null -eq $list -or $list.Items.Count -lt 900 -or -not $details.Text.Contains('RUNES')){throw 'The ESR runeword archive did not render.'}
 $search.Text = 'Holy'
 if($list.Items.Count -lt 10 -or -not $details.Text.Contains('Chipped Diamond')){throw 'The ESR runeword search did not find source entries.'}
 $atlasType.GetField('esrShowingRunewords',$flags).SetValue($app,$false)
 $atlasType.GetField('esrCategory',$flags).SetValue($app,'All entries')
 $atlasType.GetMethod('ShowEasternSunArchive',$flags).Invoke($app,@()) | Out-Null
 $panel = $atlasType.GetField('content',$flags).GetValue($app)
 $list = @($panel.Controls | Where-Object { $_.AccessibleName -eq 'ESR archive entries' })[0]
 $search = @($panel.Controls | Where-Object { $_.AccessibleName -eq 'ESR archive search' })[0]
 if($list.Items.Count -lt 500){throw 'The ESR crafting archive did not render.'}
 $search.Text = 'Tinkering'
 if($list.Items.Count -lt 5){throw 'The ESR crafting search did not find recipes.'}
 $app.Size = $app.MinimumSize
 $app.PerformLayout()
 $side = @($app.Controls | Where-Object { $_.Dock -eq [Windows.Forms.DockStyle]::Left })[0]
 $footer = @($side.Controls | Where-Object { $_.Dock -eq [Windows.Forms.DockStyle]::Bottom })[0]
 $nav = @($side.Controls | Where-Object { $_.Text -eq '13   ESR ARCHIVE' })[0]
 if($null -eq $nav -or $nav.Bounds.IntersectsWith($footer.Bounds)){throw 'The ESR Archive navigation overlaps update controls at minimum size.'}
 Write-Output 'ESR runeword and crafting archive, search and navigation passed.'
} finally {
 $app.Dispose()
 if(Test-Path $dir){Remove-Item -LiteralPath $dir -Recurse -Force}
}
