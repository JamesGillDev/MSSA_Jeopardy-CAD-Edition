# MSSA Jeopardy - CAD Edition (v2.3)

[Live Web App](https://mssa-jeopardy-jamesgill.azurewebsites.net/)

[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-Interactive%20Server-blue)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![License](https://img.shields.io/badge/License-BLS-blue.svg)](./LICENSE.md)

A fun, interactive Jeopardy-style quiz game for the **Microsoft Software & Systems Academy (MSSA) Cloud Application Development Program**. Test your knowledge across a huge range of cloud, dev, and data topics!

---

## What's New in v2.3
- **Massive Category Pool:** 117 categories including .NET Core, Git & Version Control, Cloud Security, Containers & Kubernetes, Microsoft Power Platform, Data Analytics, Azure Blob Storage, Key Vault, App Service, and many more.
- **Deep Question Pool:** Each category has at least 5 unique questions per point value (100–500), for endless replayability.
- **Randomized Board:** Each game, a random question is chosen for each point value in each selected category.
- **User-Selectable Categories:** Choose up to 6 categories at the start of each game.
- **Modern Blazor Interactive Server:** Built with .NET 8 and C# 12 for a fast, interactive, and modern web experience.
- **Modern layout and voices added using ElevenLabs.**

## Key Features
- Multiplayer support (add/remove players, custom names)
- Score tracking and winner calculation
- Bonus questions (randomly assigned)
- Reset and replay functionality
- Clean, responsive UI

## Categories (Sample)
- Azure Fundamentals, AZ-900, AZ-204, AI-900
- .NET Core, C# Programming
- Git & Version Control
- Cloud Security, Security
- Containers & Kubernetes
- Microsoft Power Platform
- Data Analytics, Databases
- Azure Blob Storage, Key Vault, App Service, App Configuration
- Container Apps, Container Registry, Service Bus, Event Grid, Event Hub, Functions
- Bicep, Application Insights, Azure Monitor, and many more!

## How to Play
1. Launch the app (Blazor Interactive Server, .NET 8 required)
2. Select up to 6 categories
3. Add players and set names
4. Start the game and take turns selecting questions
5. Answer questions, earn (or lose) points, and see who wins!

## How to Build/Run
1. Clone the repo
2. Open in Visual Studio or VS Code
3. Build and run the Blazor project

## Standalone Localhost Mode (Recommended)
This app is configured to run as a **local host service** and bind to:

- `http://127.0.0.1:8080` (default)
- Optional override via environment variable: `JEOPARDY_LOCAL_PORT`
- If a cloud runtime sets `PORT`, the app will bind to `0.0.0.0:$PORT` automatically.

### Run with .NET SDK
```powershell
dotnet run --project "MSSA Jeopardy/MSSA Jeopardy.csproj"
```

### Run on a different local port
```powershell
$env:JEOPARDY_LOCAL_PORT="5099"
dotnet run --project "MSSA Jeopardy/MSSA Jeopardy.csproj"
```

### Publish as a standalone Windows app (no .NET install required)
```powershell
dotnet publish "MSSA Jeopardy/MSSA Jeopardy.csproj" -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o "./dist/win-x64"
```
Then run:
```powershell
.\dist\win-x64\MSSA_Jeopardy.exe
```

## How It Was Deployed on Azure (Web App)
The original hosted version was deployed to **Azure App Service (Web App)** using **GitHub Actions**.

- A Web App was created in Azure (App Service) with **.NET 8** runtime.
- The app name used by the workflow is `mssa-jeopardy-jamesgill` (see `.github/workflows/main.yml`).
- A publish profile was downloaded from Azure Portal and stored in GitHub Secrets (referenced as `AZUREAPPSERVICE_PUBLISHPROFILE` in the current workflow).
- Azure deployment is gated by repository variable `ENABLE_AZURE_DEPLOY`; set it to `true` to enable the deploy job.
- On each push to `main` (or manual workflow run), GitHub Actions restores, builds, and publishes `MSSA Jeopardy/MSSA Jeopardy.csproj`.
- When `ENABLE_AZURE_DEPLOY=true`, the workflow deploys the published output using `azure/webapps-deploy@v3`.
- The live site was served at: `https://mssa-jeopardy-jamesgill.azurewebsites.net/`.

## Deploy Without Azure (Optional Cloud Hosting)
You can keep this project live without an Azure subscription.
Because this app uses **Blazor Interactive Server mode**, it must run on a server host (not static hosting like GitHub Pages).

### Option 1 (Recommended): Render
1. Push this repo to GitHub.
2. In Render, create a new **Web Service** from your GitHub repo.
3. Choose **Docker** runtime (the repo now includes a `Dockerfile`).
4. Deploy.

### Option 2: Railway or Fly.io
- Both can deploy this app directly from the same `Dockerfile`.
- Connect the repo, deploy as a web service, and keep the default container port (`8080`).

### Local Demo (No Cloud Needed)
```bash
docker build -t mssa-jeopardy .
docker run --rm -p 8080:8080 mssa-jeopardy
```
Then open `http://localhost:8080`.

## Contributing
Pull requests are welcome! Please ensure new questions are unique and not duplicates of existing ones. For major changes, open an issue first to discuss what you would like to change.

## License
This project is licensed under the **Business Source License 1.1 (BLS)**. See [LICENSE.md](./LICENSE.md) for full terms.
- **Current use grant:** Copy, modify, and redistribute for non-production use.
- **Additional Use Grant:** None.
- **Change Date:** 2029-01-01.
- **Change License:** Apache License 2.0 (effective on the Change Date, per license terms).

---
MSSA Jeopardy - CAD Edition v2.3 | Developed by JamesGillDev and contributors
