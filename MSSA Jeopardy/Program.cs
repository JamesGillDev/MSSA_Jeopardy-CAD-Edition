using MSSA_Jeopardy.Components;
using MSSA_Jeopardy.Services;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Standalone mode by default: host only on localhost.
// If a cloud host sets ASPNETCORE_URLS or PORT, respect that instead.
var explicitUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
var cloudPortSetting = Environment.GetEnvironmentVariable("PORT");
var hasCloudPort = int.TryParse(cloudPortSetting, out var cloudPort) && cloudPort is >= 1 and <= 65535;
string? localLaunchUrl = null;

if (string.IsNullOrWhiteSpace(explicitUrls))
{
    if (hasCloudPort)
    {
        builder.WebHost.UseUrls($"http://0.0.0.0:{cloudPort}");
    }
    else
    {
        const int defaultLocalPort = 8080;
        var localPortSetting = Environment.GetEnvironmentVariable("JEOPARDY_LOCAL_PORT");
        int localPort = int.TryParse(localPortSetting, out var parsedPort) && parsedPort is >= 1 and <= 65535
            ? parsedPort
            : defaultLocalPort;
        localLaunchUrl = $"http://localhost:{localPort}";
        builder.WebHost.UseUrls(localLaunchUrl);
    }
}

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    })
    .AddHubOptions(options =>
    {
        options.HandshakeTimeout = TimeSpan.FromSeconds(30);
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    });

builder.Services.AddSingleton<JeopardyGameService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

if (localLaunchUrl is not null &&
    OperatingSystem.IsWindows() &&
    Environment.UserInteractive &&
    !app.Environment.IsDevelopment() &&
    !string.Equals(Environment.GetEnvironmentVariable("JEOPARDY_NO_AUTO_LAUNCH"), "1", StringComparison.OrdinalIgnoreCase))
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = localLaunchUrl,
                UseShellExecute = true
            });
        }
        catch
        {
            // No-op: app remains available at localLaunchUrl.
        }
    });
}

app.Run();
