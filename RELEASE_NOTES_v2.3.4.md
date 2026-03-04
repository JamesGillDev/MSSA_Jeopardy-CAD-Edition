# MSSA Jeopardy! v2.3.4

## Release Notes
MSSA Jeopardy! - Cloud Application Development Edition

## Release Type
- **Public Release**

## Highlights
- Tightened ambiguous clue answers in public-release curriculum banks so host grading is more consistent.
- Embedded a real Windows icon into published `MSSA_Jeopardy.exe`.
- Standardized project open path on `MSSA_Jeopardy.sln` and cleared stale local `.slnx` cache state.
- Retained all v2.3.3 gameplay improvements (category filtering, skip-clue, and normalized audio behavior).

## Question Bank Vetting
- Completed a strictness pass for these public-release banks:
  - `AZ-900 (Azure Fundamentals)`
  - `AZ-204 (Azure Developer)`
  - `AI-900 (Azure AI Fundamentals)`
  - `Algorithms (C#)`
  - `DP-3001 (Azure Data)`
  - `DP-080 (Data Fundamentals)`
  - `DP-3020 (Advanced Data)`
  - `MS-4010 (Security)`
  - `App Service`
  - `Functions`
  - `Windows Virtual Desktop`
  - `Application Gateway`
  - `Role-Based Access Control`
  - `Multi-Factor Authentication`
- Updated these clues to single expected-answer phrasing by removing `or`/slash alternative answer wording where applicable.

## Packaging & Build
- Added `ApplicationIcon` metadata in:
  - `MSSA Jeopardy/MSSA Jeopardy.csproj`
- Added icon file:
  - `MSSA Jeopardy/app.ico`
- Verified local publish output:
  - `dotnet publish "MSSA Jeopardy/MSSA Jeopardy.csproj" -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o "./publish/v2.3.4"`

## Validation
- Build validation completed successfully:
  - `dotnet build "MSSA Jeopardy/MSSA Jeopardy.csproj" -c Release`
- Publish validation completed successfully for `win-x64` single-file output.
