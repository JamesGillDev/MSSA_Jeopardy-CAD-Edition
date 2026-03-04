# MSSA Jeopardy! v2.3.3

## Release Notes
MSSA Jeopardy! - Cloud Application Development Edition

## Release Type
- **Public Release**

## Highlights
- Audio output normalized to a safe classroom level (10% maximum).
- FX volume control now applies consistently to synthesized SFX and voice/music clips.
- Added host skip-clue flow so clues can be skipped with no points awarded.
- Added category filtering during setup to speed up board selection.
- Replaced vague parenthesized category banks with concrete, technical Jeopardy-style clue/answer material.

## Gameplay Updates
- **Skip Clue (No Score)**
  - New host button inside the clue modal: `Skip Clue (No Score)`.
  - Keyboard shortcut support: `S`.
  - Skipped clues are marked answered and removed from play with zero score impact.

- **Category Filter**
  - New setup search/filter input for category names.
  - Real-time filtering while preserving completion/selection constraints.
  - Empty-state message when no categories match the current filter.

## Audio Updates
- Added a unified volume model so the HUD FX slider now controls:
  - Web Audio SFX
  - Clip playback (`daily-double`, welcome/start/winner clips)
- Applied a global output cap of `10%` to reduce loudness spikes.
- Retained backward-compatible aliases for previous typoed clip keys (`stary-game-4`, `winnder-3`).

## Question Bank QA Sweep
- Fully replaced these categories with clearer, objective clue/answer content:
  - `AZ-900 (Azure Fundamentals)`
  - `AZ-204 (Azure Developer)`
  - `AI-900 (Azure AI Fundamentals)`
  - `Algorithms (C#)`
  - `DP-3001 (Azure Data)`
  - `DP-080 (Data Fundamentals)`
  - `DP-3020 (Advanced Data)`
  - `MS-4010 (Security)`
- Replaced additional placeholder/generic categories with technical, clue-style material:
  - `App Service`
  - `Functions`
  - `Windows Virtual Desktop`
  - `Application Gateway`
  - `Role-Based Access Control`
  - `Multi-Factor Authentication`
- Fixed typo in Event Grid clue answer:
  - `What is dead-lettering (sending to DLQ)?`

## Validation
- Build validation completed successfully:
  - `dotnet build "MSSA Jeopardy/MSSA Jeopardy.csproj" -c Release`
- Question-pool shape validation completed:
  - All categories remain complete (`25` clues each, `5` per point tier).
