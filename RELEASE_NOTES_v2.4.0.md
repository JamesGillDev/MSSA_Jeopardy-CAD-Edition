# MSSA Jeopardy! v2.4.0

## Release Notes
MSSA Jeopardy! - Cloud Application Development Edition

## Release Type
- **Public Release**

## Highlights
- Added a MAUI Blazor desktop app target for Windows (`MSSA_Jeopardy.Maui`).
- Desktop mode runs in a native app window, removing localhost URL dependency for local play.
- Preserved existing Jeopardy gameplay, categories, audio behavior, and shared service logic.
- Updated local publish workflow to produce one desktop output folder: `publish/current`.

## MAUI Blazor Migration
- New project:
  - `MSSA_Jeopardy.Maui/MSSA_Jeopardy.Maui.csproj`
- Shared logic integrated by linking existing files:
  - `Shared/JeopardyGameService.Shared.cs`
  - `MSSA Jeopardy/Models/JeopardyQuestion.cs`
- Reused gameplay UI/assets in MAUI:
  - `Pages/Home.razor`
  - `Pages/GameBoard.razor`
  - `wwwroot/audio.js`
  - `wwwroot/audio/*`
  - `wwwroot/icons/*`

## Local Publish
- Publish command:
  - `.\scripts\publish-local.ps1`
- Output:
  - `publish/current`
- Run:
  - `publish/current/MSSA_Jeopardy.Maui.exe`

## Validation
- Solution build validation completed.
- MAUI Windows publish validation completed successfully using:
  - `net8.0-windows10.0.19041.0`
  - `win10-x64`
