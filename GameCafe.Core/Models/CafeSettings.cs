using System.Text.Json;

namespace GameCafe.Core.Models;

/// <summary>
/// Persisted configuration for the gaming cafe: billing rates and launcher paths.
/// Stored as cafe-settings.json in the application base directory.
/// </summary>
public class CafeSettings
{
    public decimal HourlyRate { get; set; } = 3.00m;

    public List<LauncherConfig> Launchers { get; set; } = new()
    {
        new() {
            Key = "playnite",
            Name = "Playnite",
            Icon = "🎮",
            Enabled = true,
            ExePath = @"C:\Program Files\Playnite\Playnite.DesktopApp.exe",
            LaunchArgs = "--fullscreen",
            ProtocolUri = string.Empty
        },
        new() {
            Key = "steam",
            Name = "Steam",
            Icon = "🕹️",
            Enabled = true,
            ExePath = @"C:\Program Files (x86)\Steam\steam.exe",
            LaunchArgs = "-bigpicture",
            ProtocolUri = "steam://"
        },
        new() {
            Key = "epic",
            Name = "Epic Games",
            Icon = "⚡",
            Enabled = true,
            ExePath = @"C:\Program Files (x86)\Epic Games\Launcher\Portal\Binaries\Win32\EpicGamesLauncher.exe",
            LaunchArgs = string.Empty,
            ProtocolUri = "com.epicgames.launcher://"
        },
        new() {
            Key = "ea",
            Name = "EA App",
            Icon = "🏆",
            Enabled = true,
            ExePath = @"C:\Program Files\Electronic Arts\EA Desktop\EA Desktop\EADesktop.exe",
            LaunchArgs = string.Empty,
            ProtocolUri = string.Empty
        },
    };

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly string SettingsFilePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "cafe-settings.json");

    public static CafeSettings LoadOrDefault()
    {
        if (File.Exists(SettingsFilePath))
        {
            try
            {
                var json = File.ReadAllText(SettingsFilePath);
                return JsonSerializer.Deserialize<CafeSettings>(json) ?? new CafeSettings();
            }
            catch { }
        }
        return new CafeSettings();
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(this, JsonOptions);
        File.WriteAllText(SettingsFilePath, json);
    }
}

public class LauncherConfig
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "🎮";
    public bool Enabled { get; set; } = true;

    /// <summary>Full path to the launcher executable.</summary>
    public string ExePath { get; set; } = string.Empty;

    /// <summary>Command-line arguments passed when launching.</summary>
    public string LaunchArgs { get; set; } = string.Empty;

    /// <summary>Protocol URI fallback (e.g. steam://) when exe is not found.</summary>
    public string ProtocolUri { get; set; } = string.Empty;
}
