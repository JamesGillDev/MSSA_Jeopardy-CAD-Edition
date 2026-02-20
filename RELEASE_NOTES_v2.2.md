# MSSA Jeopardy! v2.2

## Release Notes
MSSA Jeopardy! - Cloud Application Development Edition

## Highlights
- Major game-show UX upgrade with broadcast HUD, cinematic transitions, and winner finale experience.
- New host controls for learning and classroom flow, including optional timer mode.
- Player management expanded with add/remove/rename support before game start.
- Service architecture unified to prevent drift between client/server service files.

## New Features
- **Broadcast HUD + Live Context**
  - Sticky top HUD with current player, next player, score strip, and mode status pill.
  - Round progress bar (answered/remaining clues).
  - Live event ticker for clue picks, buzzes, scoring, timeouts, mode changes, and winner.

- **Cinematic Gameplay**
  - Board reveal animations and polished clue tile states.
  - Full-screen question modal with keyboard shortcuts.
  - Round-start 3-2-1 intro overlay before board unlock.
  - Winner finale overlay with confetti, podium ranking, and rematch flow.

- **Audio & Feedback**
  - Web Audio powered SFX system.
  - Mute + volume controls in HUD.
  - Distinct SFX events for clue open, buzz, correct/incorrect, countdown ticks, timeout, and winner.

- **Timer Modes (Classroom/Learning)**
  - Pre-game **Timed Mode** toggle (on/off).
  - **Practice Mode** disables clue countdown for learning sessions.
  - Mid-game host toggle with confirmation:
    - Timed -> starts a fresh 20-second timer for open clue.
    - Practice -> stops active clue timer immediately.

- **Player Management**
  - Add players in setup.
  - Remove players in setup (minimum one player enforced).
  - Edit player names inline before start.
  - Max 9 players aligned with keyboard buzz shortcuts (`1-9`).

## Accessibility & Mobile Improvements
- Improved ARIA roles/labels and live regions for screen reader updates.
- Better keyboard focus visibility and navigation behavior.
- Enhanced mobile/touch layout behavior for board, modals, and controls.
- Reduced-motion support respected for animated scenes.

## Architecture/Engineering Updates
- `JeopardyGameService` moved to shared source (`Shared/JeopardyGameService.Shared.cs`) and linked into the web project to keep service logic in one canonical file.
- Wrapper service files remain as partial stubs to avoid duplicate logic and namespace drift.
- Category canonicalization/completeness logic preserved in shared service for stable board generation.

## Documentation & Licensing
- README updated with live web app link at top.
- README license references now point to local `LICENSE.md`.
- Added BLS summary to README (current use grant, additional grant, change date, change license).

## Build/Validation
- Release build validated successfully:
  - `dotnet build "MSSA Jeopardy/MSSA Jeopardy.csproj" -c Release --no-restore`

## Bug Fixes
- Resolved service duplication/drift risk by unifying shared game service source.
- Prevented timer-related classroom friction by introducing Practice Mode.
