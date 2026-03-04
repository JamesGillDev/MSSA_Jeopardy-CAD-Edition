# MSSA Jeopardy! v2.3.5

## Release Notes
MSSA Jeopardy! - Cloud Application Development Edition

## Release Type
- **Public Release**

## Highlights
- Removed duplicate category families from the visible picker by canonicalizing aliases.
- Updated local published EXE startup behavior to bind to `localhost` and auto-open the app in your default browser.
- Retained prior v2.3.4 quality improvements (Q/A strictness pass, EXE icon embedding, and solution cache cleanup).

## Duplicate Category Cleanup
- Implemented canonical alias mapping in shared game service category generation and selection.
- Duplicate alias examples now collapsed to one canonical option:
  - `Azure Fundamentals` -> `AZ-900 (Azure Fundamentals)`
  - `Azure Application Gateway` -> `Application Gateway`
  - `Azure Functions` -> `Functions`
  - `Azure Key Vault` -> `Key Vault`
  - `Blob Storage` -> `Azure Blob Storage`
  - `Azure Sentinel` -> `Microsoft Sentinel`
  - `Azure Security Center` -> `Microsoft Defender for Cloud`
  - `Azure Logs` -> `Log Analytics`
- Result: category picker now shows canonical banks only (reduced from `117` to `109` visible categories).

## Local Publish Behavior
- Local published executable now:
  - Uses `http://localhost:<port>` (loopback-only on your PC).
  - Auto-launches your default browser after startup in production/local mode.
  - Supports opt-out with `JEOPARDY_NO_AUTO_LAUNCH=1`.

## Validation
- Build validation completed successfully:
  - `dotnet build "MSSA Jeopardy/MSSA Jeopardy.csproj" -c Release`
- Local publish validation completed successfully:
  - `dotnet publish "MSSA Jeopardy/MSSA Jeopardy.csproj" -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o "./publish/v2.3.5"`
