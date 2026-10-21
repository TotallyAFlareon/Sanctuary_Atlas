# Live content updates

Update 29 can refresh runeword effects and guide book regions while the app remains open. Program changes still require a new installer and restart. The user checks for updates from the bottom corner.

`SanctuaryAtlasContent.xml` is a complete snapshot for one program version. It must include every existing runeword and can include a `guidebook` element containing `region` entries. Each region has an `act` (1–5), a unique `name`, and exactly one non-empty `details` element. A guide entry replaces the bundled entry with that name, or adds a new entry. Entries omitted from the package fall back to the guide bundled with that program version. This is whole-snapshot replacement, so preserve all prior content changes in subsequent packages.

`BuildContentPackage.cs` generates a starting snapshot from `runeword-effects.xml` and the bundled guide in the compiled application. On Windows, after building the app, compile this utility referencing `Sanctuary Atlas.exe`, then run it with the source and destination XML filenames. Edit the resulting XML, preserving runewords, and validate it with `LiveContentCheck.cs`. Do not generate a new baseline over existing content changes without merging them.

For a content-only update, provide the updated `SanctuaryAtlasContent.xml` asset on the latest release **without changing its version tag or installer**. GitHub must expose the asset's SHA-256 digest. The app downloads and checks the digest, validates the complete package, saves it, and refreshes the open guide or runeword library. Versions through 28 ignore guide entries; version 29 or later is required for guide refreshes. Content intended for a different program version must not be published as a content-only update.

User builds and notes are stored separately from content. A failed validation leaves the previous content in place. This mechanism does not execute new code or update skill formulas, equipment validation, or other application behavior.
