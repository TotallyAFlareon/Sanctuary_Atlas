$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path 'Sanctuary Atlas.exe').Path)
$guide = $assembly.GetType('ShoppingGuide')
$topics = $guide.GetField('Topics').GetValue($null)
if ($topics.Length -ne 14) { throw 'Shopping topics missing.' }
$details = $topics[2].Details(33)
if (-not ($details.Contains('Shop example: reached') -and $details.Contains('need 1 more level') -and $details.Contains('item level 38'))) { throw 'Level 33 boot guidance is incorrect.' }
$details = $topics[2].Details(43)
if (-not ($details.Contains('At level 43') -and $details.Contains('threshold: reached'))) { throw 'Gambling target status is incorrect.' }
if (-not $topics[3].Details(57).Contains('magic-only')) { throw '40% boots must identify the magic-only affix.' }
if (-not ($topics[4].Details(95).Contains('level 95') -and $topics[5].Details(92).Contains('level 92'))) { throw 'Jewelry thresholds are missing.' }
$mf=@($topics | Where-Object { $_.Name -like 'MF *' })
if ($mf.Count -ne 4) { throw 'Magic find targets missing.' }
foreach($case in @(@(6,12,13,22),@(7,22,23,32),@(8,0,38,47),@(9,0,22,31))){
 $topic=$topics[$case[0]]
 if($topic.ShopLevel -ne $case[1] -or $topic.GambleFirst -ne $case[2] -or $topic.GambleAll -ne $case[3]) { throw "Wrong magic find thresholds for $($topic.Name)" }
 if(-not $topic.Details($case[2]-1).Contains('Gamble: need 1 more level')) { throw 'First gamble threshold boundary failed.' }
 if(-not $topic.Details($case[3]).Contains('threshold: reached')) { throw 'All gamble threshold boundary failed.' }
}
if(-not $topics[8].Description.Contains('ring version of Fortune') -or -not $topics[9].Description.Contains('Magic Amulet') -and -not $topics[9].Description.Contains('magic Fortuitous Amulet')){throw 'Magic find jewelry guidance missing.'}
$leveling = $topics[11].Details(30)
if(-not $leveling.Contains('LEVELING TARGETS')){throw 'Magic find leveling guidance missing.'}
$dir = Join-Path (Get-Location) ('test-builds\shopping-' + [Guid]::NewGuid().ToString('N'))
$app = New-Object Atlas($dir)
try {
 $flags = [Reflection.BindingFlags]'Instance,NonPublic'
 $assembly.GetType('Atlas').GetMethod('ShowShoppingGuide', $flags).Invoke($app, @()) | Out-Null
 $panel = $assembly.GetType('Atlas').GetField('content', $flags).GetValue($app)
 $list = @($panel.Controls | Where-Object { $_.AccessibleName -eq 'Shopping guide topics' })[0]
 $view = @($panel.Controls | Where-Object { $_.AccessibleName -eq 'Shopping guide details' })[0]
 $level = @($panel.Controls | Where-Object { $_.AccessibleName -eq 'Shopping guide character level' })[0]
 if ($null -eq $list -or $null -eq $view -or $null -eq $level -or $list.Items.Count -ne 14) { throw 'Shopping interface missing.' }
 $list.SelectedIndex = 2
 $level.Value = 43
 if (-not $view.Text.Contains('threshold: reached')) { throw 'Changing the character level did not update the selected guide.' }
 $list.SelectedIndex = 6
 $level.Value = 13
 if (-not $view.Text.Contains('Gamble: reached') -or -not $view.Text.Contains('Elzix')) { throw 'Magic find guide did not update in the interface.' }
 $list.SelectedIndex = 11
 if (-not $view.Text.Contains('Normal difficulty') -or -not $view.Text.Contains('no fixed breakpoint') -or -not $view.Text.Contains('diminishing') -or -not $view.Text.Contains('rune drops') -or -not $view.Text.Contains('resistances')) { throw 'Magic find leveling guidance did not appear in the interface.' }
 $build = $assembly.GetType('Atlas').GetField('build', $flags).GetValue($app)
 if ($build.Level -ne 30) { throw 'Shopping level changed the saved build.' }
 $app.Size = $app.MinimumSize
 $app.PerformLayout()
 $side = @($app.Controls | Where-Object { $_.Dock -eq [Windows.Forms.DockStyle]::Left })[0]
 $footer = @($side.Controls | Where-Object { $_.Dock -eq [Windows.Forms.DockStyle]::Bottom })[0]
 $nav = @($side.Controls | Where-Object { $_.Text -eq '09   SHOPPING GUIDE' })[0]
 if ($null -eq $nav -or $nav.Bounds.IntersectsWith($footer.Bounds)) { throw 'Shopping navigation overlaps the update controls at minimum size.' }
 Write-Output 'Shopping thresholds, level selection and guide navigation passed.'
} finally { $app.Dispose() }

