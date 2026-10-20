# Sanctuary Atlas source

Build on Windows with .NET Framework 4 using PowerShell: run .\Build.ps1.
The result is SanctuaryAtlasSetup.exe. The app stores per-class builds in the current user's Local AppData.

The default update source is TotallyAFlareon/Sanctuary_Atlas. The updater checks public GitHub releases without credentials. GitHub must expose the repository and releases publicly before update checks work.
For a release, attach SanctuaryAtlasSetup.exe to a release named with an integer version tag (for example, v12). Update AtlasUpdates.CurrentVersion for each app version.

Runeword effects derive from Fabrice Denis's MIT-licensed Runewizard via https://github.com/Jekman87/diablo2-runeword-tracker. See runewizard-LICENSE.txt.
This is an unofficial Diablo II fan project.

Live runeword content: attach SanctuaryAtlasContent.xml (the contents of runeword-effects.xml) to every program release. Update 17 and later check its GitHub SHA-256 digest and can apply changes for the current program version without restarting. The file must contain every runeword already known to the app; new runewords, skills, recipes, and other program changes still require a new program release. To make a content-only update, replace the SanctuaryAtlasContent.xml asset on the latest release without changing its version tag; keep the installer asset intact. Program releases must use a higher vNN tag and a compatible content asset. Users can change or disable the release source from the small settings button next to the corner update control.