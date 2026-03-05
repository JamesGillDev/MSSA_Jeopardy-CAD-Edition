# MSSA Jeopardy! v2.4.1

## Release Notes
MSSA Jeopardy! - Cloud Application Development Edition

## Release Type
- **Public Release**

## Highlights
- Refreshed the desktop icon with a custom Jeopardy-themed visual style.
- Embedded a dedicated Windows executable icon so published `MSSA_Jeopardy.Maui.exe` no longer shows the default .NET icon in File Explorer.
- Restored full MAUI desktop Jeopardy styling by re-enabling Blazor CSS isolation.
- Fixed persistent bottom-left unhandled error banner visibility in desktop mode.
- Bumped MAUI desktop version metadata and versioned audio marker to `2.4.1`.
- Fixed published version badge rendering so it shows only the public release version (for example `v2.4.1`) without `+build` metadata.
- Fixed Daily Double clue modal layout so player judging controls stay in normal position.

## Fix Details
- MAUI project update:
  - Replaced `Resources/appicon.svg` and `Resources/appiconfg.svg` artwork with custom Jeopardy icon assets used for desktop app icon/splash generation.
  - Added `Resources/app-win32.ico` and set `ApplicationIcon` in `MSSA_Jeopardy.Maui.csproj` so the unpackaged published EXE carries Jeopardy branding.
  - Removed `EnableDefaultCssItems=false` from `MSSA_Jeopardy.Maui.csproj`.
  - Added an MSBuild target that maps MAUI `@(MauiCss)` items into `@(ScopedCssInput)` so `MSSA_Jeopardy.Maui.styles.css` is generated and published reliably.
- Global error UI handling:
  - Added default `#blazor-error-ui` hide styles in `MSSA_Jeopardy.Maui/wwwroot/app.css`.
- Version synchronization:
  - Updated `MSSA_Jeopardy.Maui/wwwroot/index.html` audio script query to `v=2.4.1`.
  - Updated `MSSA_Jeopardy.Maui/wwwroot/audio.js` runtime version marker to `2.4.1`.
  - Updated Home release badge version parsing in both web and MAUI pages to trim `AssemblyInformationalVersion` metadata after `+`, rendering `Published v2.4.1` style output.
- Daily Double modal flow:
  - Updated question modal card layout from fixed grid rows to a flexible column flow so optional Daily Double banner rows do not force judge controls to the bottom.
  - Applied the same fix to desktop runtime fallback stylesheet (`wwwroot/maui-components.css`) used by the published MAUI app.

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
