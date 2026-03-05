# Changelog
All notable changes to this project are documented in this file for public GitHub releases.

Older tags use short semantic forms (`v2.1`, `v2.2`, `v2.3`). This changelog normalizes those entries as `2.1.0`, `2.2.0`, and `2.3.0`.

## [2.4.1] - 2026-03-04
### Added
- Added `RELEASE_NOTES_v2.4.1.md` for this public release.

### Changed
- Bumped MAUI desktop app version metadata from `2.4.0` to `2.4.1`.
- Updated versioned desktop audio asset marker to `2.4.1`.

### Fixed
- Restored MAUI Blazor CSS isolation so desktop views render with the intended styles.
- Added global `#blazor-error-ui` defaults so the unhandled error banner is hidden unless a real runtime fault occurs.
- Updated published version badge output to show only the release version (removed `+build` metadata suffix).
- Fixed Daily Double clue modal layout so the judge/answer controls remain in normal flow instead of being pushed to the bottom.

## [2.4.0] - 2026-03-04
### Added
- Added a new Windows-targeted MAUI Blazor desktop project: `MSSA_Jeopardy.Maui`.
- Reused core gameplay/question-bank logic in desktop mode via linked shared service/model files.
- Added `RELEASE_NOTES_v2.4.0.md` for this public release.

### Changed
- Updated local publish workflow (`scripts/publish-local.ps1`) to publish the MAUI desktop app to `publish/current`.
- Updated README run/publish guidance to desktop-first usage.

### Fixed
- Eliminated localhost dependency for local classroom runs by enabling native desktop hosting.

## [2.3.5] - 2026-03-04
### Added
- Added canonical category alias handling to collapse duplicate category families in selection lists.
- Added `RELEASE_NOTES_v2.3.5.md` for this public release.

### Changed
- Updated category discovery and completeness lists to show canonical names only (removed duplicate aliases like `Azure Fundamentals` when `AZ-900 (Azure Fundamentals)` is present).
- Bumped application/release metadata from `2.3.4` to `2.3.5`.

### Fixed
- Improved local published EXE behavior to use `localhost` and auto-open the default browser on startup.

## [2.3.4] - 2026-03-04
### Added
- Embedded a Windows application icon for published executables using `app.ico` and project `ApplicationIcon` metadata.
- Added `RELEASE_NOTES_v2.3.4.md` for the new public release.

### Changed
- Tightened ambiguous multi-answer wording in the public-release curriculum banks to improve strict, classroom-friendly clue evaluation.
- Bumped application/release metadata from `2.3.3` to `2.3.4`.

### Fixed
- Cleared stale local `.slnx` cache state and standardized solution loading on `MSSA_Jeopardy.sln`.

## [2.3.3] - 2026-03-04
### Added
- Category filtering on the game setup screen to quickly narrow category choices.
- Host skip-clue action (UI button + `S` hotkey) that awards no points.
- Published release icon asset and Home screen `Published vX` badge.

### Changed
- Normalized app audio to a 10% output ceiling and unified FX slider behavior across synthesized SFX and clip playback.
- Rewrote vague parenthesized category banks with concrete technical clue/answer content:
  - `AZ-900 (Azure Fundamentals)`
  - `AZ-204 (Azure Developer)`
  - `AI-900 (Azure AI Fundamentals)`
  - `Algorithms (C#)`
  - `DP-3001 (Azure Data)`
  - `DP-080 (Data Fundamentals)`
  - `DP-3020 (Advanced Data)`
  - `MS-4010 (Security)`
- Replaced additional generic/template category banks with specific technical clue/answer content:
  - `App Service`
  - `Functions`
  - `Windows Virtual Desktop`
  - `Application Gateway`
  - `Role-Based Access Control`
  - `Multi-Factor Authentication`
- Bumped application/release metadata to `v2.3.3` and synced versioned audio asset marker.

### Fixed
- Corrected typo in Event Grid Q/A (`What is dead-lettering ...`).

## [2.3.2] - 2026-02-26
### Changed
- Refined deployment workflow/docs updates since `v2.3.1` for clearer cloud-host publishing on GitHub.
- Added explicit project version metadata (`Version`, `AssemblyVersion`, `FileVersion`, `InformationalVersion`) in the web app project.
- Added this consolidated changelog so public releases show multiple iterations of code change in one place.
- Synced runtime asset version markers to `2.3.2` for consistent release tracking.

## [2.3.1] - 2026-02-25
### Added
- Cloud-host deployment support and Azure deployment documentation updates.

## [2.3.0] - 2026-02-23
### Added
- Expanded content set with 117 categories and deeper per-category question pools.
- Randomized board generation and category selection flow (up to 6 categories).
- Updated modern layout and ElevenLabs voice enhancements.

## [2.2.0] - 2026-02-20
### Added
- Broadcast HUD, live context ticker, cinematic round transitions, and winner finale experience.
- Optional timed/practice modes and expanded host controls for classroom gameplay.
- Improved player setup flow (add/remove/rename before game start).

### Changed
- Unified game service architecture by sharing canonical logic from `Shared/JeopardyGameService.Shared.cs`.
- Accessibility and mobile usability improvements across board, modal, and controls.

## [2.1.0] - 2026-02-18
### Changed
- Streamlined project structure by removing older component layout paths.

## [2.0.0] - 2026-02-10
### Changed
- Initial v2 public release baseline with expanded category set and updated game content/docs.

[2.4.1]: https://github.com/JamesGillDev/MSSA_Jeopardy-CAD-Edition/compare/v2.4.0...HEAD
[2.4.0]: https://github.com/JamesGillDev/MSSA_Jeopardy-CAD-Edition/compare/v2.3.5...v2.4.0
[2.3.5]: https://github.com/JamesGillDev/MSSA_Jeopardy-CAD-Edition/compare/v2.3.4...v2.3.5
[2.3.4]: https://github.com/JamesGillDev/MSSA_Jeopardy-CAD-Edition/compare/v2.3.3...v2.3.4
[2.3.3]: https://github.com/JamesGillDev/MSSA_Jeopardy-CAD-Edition/compare/v2.3.2...v2.3.3
[2.3.2]: https://github.com/JamesGillDev/MSSA_Jeopardy-CAD-Edition/compare/v2.3.1...v2.3.2
[2.3.1]: https://github.com/JamesGillDev/MSSA_Jeopardy-CAD-Edition/compare/v2.3...v2.3.1
[2.3.0]: https://github.com/JamesGillDev/MSSA_Jeopardy-CAD-Edition/compare/v2.2...v2.3
[2.2.0]: https://github.com/JamesGillDev/MSSA_Jeopardy-CAD-Edition/compare/v2.1...v2.2
[2.1.0]: https://github.com/JamesGillDev/MSSA_Jeopardy-CAD-Edition/compare/v2.0.0...v2.1
[2.0.0]: https://github.com/JamesGillDev/MSSA_Jeopardy-CAD-Edition/releases/tag/v2.0.0
