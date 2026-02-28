using System.Diagnostics;
using GameCafe.Core.Models;

namespace GameCafe.Core.Services;

/// <summary>
/// Service for interacting with Playnite game library and launching games.
/// Communicates with Playnite via its public API and Windows process management.
/// </summary>
public interface IPlayniteIntegrationService
{
    /// <summary>
    /// Launches a game by name via Playnite.
    /// </summary>
    Task<bool> LaunchGameAsync(string gameName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of games available in Playnite library.
    /// Note: Requires Playnite to be running.
    /// </summary>
    Task<List<GameInfo>> GetAvailableGamesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if Playnite process is running.
    /// </summary>
    bool IsPlayniteRunning();

    /// <summary>
    /// Starts Playnite in fullscreen kiosk mode.
    /// </summary>
    Task<bool> StartPlayniteKioskAsync(CancellationToken cancellationToken = default);
}

public class GameInfo
{
    public string Name { get; set; } = string.Empty;
    public string? Platform { get; set; }
    public string? Category { get; set; }
    public DateTime? LastPlayed { get; set; }
}

public class PlayniteIntegrationService : IPlayniteIntegrationService
{
    private const string PlayniteProcessName = "Playnite";
    private const string PlayniteExePath = "C:\\Program Files\\Playnite\\Playnite.DesktopApp.exe";
    private readonly string _playniteDataPath;

    public PlayniteIntegrationService()
    {
        _playniteDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Playnite");
    }

    public bool IsPlayniteRunning()
    {
        var processes = Process.GetProcessesByName(PlayniteProcessName);
        return processes.Length > 0;
    }

    public async Task<bool> LaunchGameAsync(string gameName, CancellationToken cancellationToken = default)
    {
        try
        {
            // Playnite supports launching games via command line with URI scheme
            // playnite://launch/GameId or via menu search
            if (!IsPlayniteRunning())
            {
                await StartPlayniteKioskAsync(cancellationToken);
                await Task.Delay(3000, cancellationToken); // Wait for Playnite to initialize
            }

            // Use Playnite's game search and launch functionality
            // This would typically integrate with Playnite's API once fully documented
            // For MVP, we'll use process-based approach
            var process = new ProcessStartInfo
            {
                FileName = PlayniteExePath,
                Arguments = $"--launch-game \"{gameName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var proc = Process.Start(process))
            {
                return proc != null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error launching game {gameName}: {ex.Message}");
            return false;
        }
    }

    public async Task<List<GameInfo>> GetAvailableGamesAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement via Playnite SDK when plugin interface is finalized
        // For now, return empty list
        return await Task.FromResult(new List<GameInfo>());
    }

    public async Task<bool> StartPlayniteKioskAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var process = new ProcessStartInfo
            {
                FileName = PlayniteExePath,
                Arguments = "--fullscreen", // Playnite fullscreen/kiosk mode
                UseShellExecute = false
            };

            var proc = Process.Start(process);
            return await Task.FromResult(proc != null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error starting Playnite: {ex.Message}");
            return await Task.FromResult(false);
        }
    }
}
