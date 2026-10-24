# Sanctuary Atlas source

Build on Windows with .NET Framework 4 by running `.\Build.ps1` in PowerShell. The output is `SanctuaryAtlasSetup.exe`.

Sanctuary Atlas stores per-class builds in the current user's Local AppData. Its default update source is `TotallyAFlareon/Sanctuary_Atlas`; updates read public GitHub releases and do not require player credentials.

## Releases

Attach `SanctuaryAtlasSetup.exe` and `SanctuaryAtlasContent.xml` to each program release. Use an integer version tag such as `v40`, and update `AtlasUpdates.CurrentVersion` for every app release.

The app verifies each release asset with GitHub's SHA-256 digest before using it. Installer releases update the program and restart once. Content-only releases can refresh runewords, Guide Book entries, skill text, and Farming Planner routes while the app remains open. A content package must include every existing runeword.

## Data and attribution

Runeword effects derive from Fabrice Denis's MIT-licensed Runewizard via https://github.com/Jekman87/diablo2-runeword-tracker; see `runewizard-LICENSE.txt`.

This is an unofficial Diablo II fan project.
