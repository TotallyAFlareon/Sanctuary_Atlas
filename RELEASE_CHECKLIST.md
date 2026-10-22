# Release and security checks

Before publishing an update:

- Review changes to the installer, updater, network access and file handling. Keep release source and build inputs traceable to the release tag.
- Build in a dedicated workspace. Do not package test executables or user build data with release assets. Prefer focused tests that do not need the entire application compiled into each test executable.
- Run the applicable tests and record what passed. If an executable is blocked, quarantined or triggers an antivirus alert, stop using that file and report the event; do not describe the check as passed.
- Scan the exact release installer and the app it contains with current antivirus definitions. Record the scan result and hashes. A clean scan is useful evidence, not a guarantee of safety. Do not turn off protection, create exclusions, or restore quarantined files to complete a release.
- Verify the uploaded installer hash matches the scanned file and check the release assets and tag. If a scan cannot be completed, say so rather than claiming a clean result.

A detection can be a false positive, but do not assume that without further investigation. Follow the antivirus provider's quarantine and submission guidance.
