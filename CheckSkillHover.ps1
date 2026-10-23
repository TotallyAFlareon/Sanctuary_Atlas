$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path 'Sanctuary Atlas.exe').Path)
$appType = $assembly.GetType('Atlas')
$flags = [Reflection.BindingFlags]'Instance,NonPublic'
$app = New-Object Atlas((Join-Path (Get-Location) ('test-builds\hover-' + [Guid]::NewGuid().ToString('N'))))
try {
 $tip = $appType.GetField('skillTip', $flags).GetValue($app)
 if (-not $tip.OwnerDraw) { throw 'Skill hover is not custom drawn.' }
 $cardPanel = $appType.GetField('skillList', $flags).GetValue($app)
 $card = @($cardPanel.Controls | Where-Object { $_ -is [Windows.Forms.Panel] })[0]
 $text = $tip.GetToolTip($card)
 if (-not $text.Contains('Summon Goatman')) { throw 'Skill preview is missing.' }
 $sizeEvent = New-Object Windows.Forms.PopupEventArgs($app, $card, $false, ([Drawing.Size]::new(1,1)))
 $tip.GetType().GetMethod('SizeTip',$flags).Invoke($tip,[object[]]@($tip.PSObject.BaseObject,$sizeEvent.PSObject.BaseObject))
 if ($sizeEvent.ToolTipSize.Width -gt 480 -or $sizeEvent.ToolTipSize.Height -gt [Windows.Forms.Screen]::FromControl($card).WorkingArea.Height) { throw 'Hover exceeds the screen.' }
 $bitmap = New-Object Drawing.Bitmap($sizeEvent.ToolTipSize.Width,$sizeEvent.ToolTipSize.Height)
 $graphics = [Drawing.Graphics]::FromImage($bitmap)
 try {
  $bounds = [Drawing.Rectangle]::new(0,0,$bitmap.Width,$bitmap.Height)
  $drawEvent = New-Object Windows.Forms.DrawToolTipEventArgs($graphics,$app,$card,$bounds,$text,[Drawing.Color]::Black,[Drawing.Color]::White,$app.Font)
  $tip.GetType().GetMethod('DrawTip',$flags).Invoke($tip,[object[]]@($tip.PSObject.BaseObject,$drawEvent.PSObject.BaseObject))
  if ($bitmap.GetPixel(10,10).GetBrightness() -gt 0.4) { throw 'Tooltip background is not dark.' }
  $bitmap.Save((Join-Path (Get-Location) 'skill-hover-preview.png'))
 } finally { $graphics.Dispose(); $bitmap.Dispose() }
 $tip.SetToolTip($card, $text + (("`nLong skill description for wrapping and screen-height checks.") * 200))
 $longEvent = New-Object Windows.Forms.PopupEventArgs($app, $card, $false, ([Drawing.Size]::new(1,1)))
 $tip.GetType().GetMethod('SizeTip',$flags).Invoke($tip,[object[]]@($tip.PSObject.BaseObject,$longEvent.PSObject.BaseObject))
 if ($longEvent.ToolTipSize.Width -gt 480 -or $longEvent.ToolTipSize.Height -gt [Windows.Forms.Screen]::FromControl($card).WorkingArea.Height) { throw 'Long hover exceeds the screen.' }
 $tip.SetToolTip($card, $text)
 $app.Size = [Drawing.Size]::new(1920,1080); $app.PerformLayout()
 $appType.GetMethod('LayoutSkills',$flags).Invoke($app,@())
 if ($tip.Active) { throw 'Narrow tooltip should be hidden when inline detail is available.' }
 $app.Size = $app.MinimumSize; $app.PerformLayout()
 $appType.GetMethod('LayoutSkills',$flags).Invoke($app,@())
 if (-not $tip.Active) { throw 'Tooltip did not return after narrowing the window.' }
 $appType.GetMethod('ShowShoppingGuide',$flags).Invoke($app,@())
 if ($tip.GetToolTip($card)) { throw 'Old skill tooltip remained after leaving the skill page.' }
 Write-Output 'Hover styling, skill text, window sizing and navigation checks passed.'
} finally { $app.Dispose() }

