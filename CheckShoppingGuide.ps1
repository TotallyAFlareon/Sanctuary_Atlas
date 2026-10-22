$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path 'Sanctuary Atlas.exe').Path)
$guide = $assembly.GetType('ShoppingGuide')
$topics = $guide.GetField('Topics').GetValue($null)
if ($topics.Length -ne 8) { throw 'Shopping topics missing.' }
$details = $topics[2].Details(33)
if (-not ($details.Contains('Shop example: reached') -and $details.Contains('need 1 more level') -and $details.Contains('item level 38'))) { throw 'Level 33 boot guidance is incorrect.' }
$details = $topics[2].Details(43)
if (-not ($details.Contains('At level 43') -and $details.Contains('threshold: reached'))) { throw 'Gambling target status is incorrect.' }
if (-not $topics[3].Details(57).Contains('magic-only')) { throw '40% boots must identify the magic-only affix.' }
if (-not ($topics[4].Details(95).Contains('level 95') -and $topics[5].Details(92).Contains('level 92'))) { throw 'Jewelry thresholds are missing.' }
$dir = Join-Path (Get-Location) ('test-builds\shopping-' + [Guid]::NewGuid().ToString('N'))
$app = New-Object Atlas($dir)
try {
 $flags = [Reflection.BindingFlags]'Instance,NonPublic'
 $assembly.GetType('Atlas').GetMethod('ShowShoppingGuide', $flags).Invoke($app, @()) | Out-Null
 $panel = $assembly.GetType('Atlas').GetField('content', $flags).GetValue($app)
 $list = @($panel.Controls | Where-Object { $_.AccessibleName -eq 'Shopping guide topics' })[0]
 $view = @($panel.Controls | Where-Object { $_.AccessibleName -eq 'Shopping guide details' })[0]
 $level = @($panel.Controls | Where-Object { $_.AccessibleName -eq 'Shopping guide character level' })[0]
 if ($null -eq $list -or $null -eq $view -or $null -eq $level -or $list.Items.Count -ne 8) { throw 'Shopping interface missing.' }
 $list.SelectedIndex = 2
 $level.Value = 43
 if (-not $view.Text.Contains('threshold: reached')) { throw 'Changing the character level did not update the selected guide.' }
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

