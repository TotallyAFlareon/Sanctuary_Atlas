# Sanctuary Atlas source

Build on Windows with .NET Framework 4 using PowerShell: run .\Build.ps1.
The result is Sanctuary Atlas Setup.exe. The app stores per-class builds in the current user's Local AppData.

The default update source is TotallyAFlareon/Sanctuary_Atlas. The updater checks public GitHub releases without credentials. GitHub must expose the repository and releases publicly before update checks work.
For a release, attach Sanctuary Atlas Setup.exe to a release named with an integer version tag (for example, v12). Update AtlasUpdates.CurrentVersion for each app version.

Runeword effects derive from Fabrice Denis's MIT-licensed Runewizard via https://github.com/Jekman87/diablo2-runeword-tracker. See runewizard-LICENSE.txt.
This is an unofficial Diablo II fan project.
