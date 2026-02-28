using System.Diagnostics;
using GameCafe.Core.Models;
using GameCafe.Core.Security;
using GameCafe.Core.Services;

namespace GameCafe.StationAgent;

/// <summary>
/// Full-screen kiosk overlay. Flow: Login → Launcher Selection → Session overlay (timer + cost).
/// </summary>
public class KioskOverlayForm : Form
{
    private readonly IAuthenticationService _authService;
    private readonly ISessionService _sessionService;
    private readonly IBillingService _billingService;
    private readonly CafeSettings _settings;

    private User? _currentUser;
    private Session? _currentSession;
    private System.Windows.Forms.Timer? _sessionTimer;
    private int _stationId = 1;

    // Background accent colours for each launcher card
    private static readonly Dictionary<string, Color> LauncherColors = new()
    {
        ["playnite"] = Color.FromArgb(24, 80, 180),
        ["steam"]    = Color.FromArgb(23, 48,  74),
        ["epic"]     = Color.FromArgb(40, 40,  40),
        ["ea"]       = Color.FromArgb(200, 40,  10),
    };

    public KioskOverlayForm()
    {
        var passwordHasher = new PasswordHasher();
        _authService   = new AuthenticationService(passwordHasher);
        _sessionService = new SessionService();
        _billingService = new BillingService();
        _settings       = CafeSettings.LoadOrDefault();

        FormBorderStyle = FormBorderStyle.None;
        ControlBox  = false;
        KeyPreview  = true;
        Text        = "Gaming Cafe Kiosk";
        BackColor   = Color.Black;
        StartPosition = FormStartPosition.Manual;
        KeyDown    += KioskForm_KeyDown;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        var screen = Screen.PrimaryScreen ?? Screen.AllScreens[0];
        Bounds      = screen.Bounds;
        WindowState = FormWindowState.Maximized;
        TopMost     = true;
        ShowLoginScreen();
    }

    // ── Ctrl+Shift+Q → force-end session / admin exit ──────────────────
    private void KioskForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.Shift && e.KeyCode == Keys.Q)
        {
            e.Handled = true;
            LogoutAndShowLogin();
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // SCREEN 1: Login
    // ─────────────────────────────────────────────────────────────────────
    private void ShowLoginScreen()
    {
        SuspendLayout();
        Controls.Clear();

        var bg = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 20, 30) };

        var title = MakeLabel("🎮  GAMING CAFE", 48, FontStyle.Bold, Color.White,
            new Point(0, 80), ClientSize.Width, 80, ContentAlignment.TopCenter);

        var sub = MakeLabel("Sign in to start your session", 14, FontStyle.Regular,
            Color.Gray, new Point(0, 170), ClientSize.Width, 30, ContentAlignment.TopCenter);

        int cx = (ClientSize.Width - 320) / 2;

        var userLbl  = MakeLabel("Username", 13, FontStyle.Regular, Color.White, new Point(cx, 240), 320, 24);
        var userBox  = MakeTextBox(new Point(cx, 266), 320, false);
        var passLbl  = MakeLabel("Password", 13, FontStyle.Regular, Color.White, new Point(cx, 316), 320, 24);
        var passBox  = MakeTextBox(new Point(cx, 342), 320, true);
        var statusLbl = MakeLabel("Enter your credentials to continue", 12, FontStyle.Regular,
            Color.Gray, new Point(0, ClientSize.Height - 50), ClientSize.Width, 40,
            ContentAlignment.MiddleCenter);

        var loginBtn = new Button
        {
            Text      = "LOGIN",
            Font      = new Font("Segoe UI", 16, FontStyle.Bold),
            Width     = 320, Height = 52,
            Location  = new Point(cx, 408),
            BackColor = Color.FromArgb(0, 150, 255),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        loginBtn.FlatAppearance.BorderSize = 0;
        loginBtn.Click += async (s, e) =>
        {
            loginBtn.Enabled = false;
            loginBtn.Text    = "Signing in…";
            await HandleLogin(userBox.Text, passBox.Text, statusLbl);
            loginBtn.Enabled = true;
            loginBtn.Text    = "LOGIN";
        };

        // Allow pressing Enter from password box
        passBox.KeyDown += async (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.Handled = true; await HandleLogin(userBox.Text, passBox.Text, statusLbl); }
        };

        bg.Controls.AddRange(new Control[] { title, sub, userLbl, userBox, passLbl, passBox, loginBtn, statusLbl });
        Controls.Add(bg);
        ResumeLayout();
        userBox.Focus();
    }

    private async Task HandleLogin(string username, string password, Label statusLabel)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            statusLabel.Text      = "Please enter your username and password.";
            statusLabel.ForeColor = Color.OrangeRed;
            return;
        }

        try
        {
            var result = await _authService.LoginAsync(username, password);
            if (!result.Success || result.User == null)
            {
                statusLabel.Text      = $"Login failed: {result.Message}";
                statusLabel.ForeColor = Color.OrangeRed;
                return;
            }

            _currentUser    = result.User;
            _currentSession = await _sessionService.CreateSessionAsync(_currentUser.Id, _stationId, "Kiosk");
            ShowLauncherSelection();
        }
        catch (Exception ex)
        {
            statusLabel.Text      = $"Error: {ex.Message}";
            statusLabel.ForeColor = Color.OrangeRed;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // SCREEN 2: Launcher Selection
    // ─────────────────────────────────────────────────────────────────────
    private void ShowLauncherSelection()
    {
        // Ensure we're back to fullscreen when showing the picker
        var screen = Screen.PrimaryScreen ?? Screen.AllScreens[0];
        FormBorderStyle = FormBorderStyle.None;
        Bounds      = screen.Bounds;
        WindowState = FormWindowState.Maximized;
        TopMost     = true;

        SuspendLayout();
        Controls.Clear();

        var bg = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(15, 15, 25) };

        var title = MakeLabel($"Welcome, {_currentUser?.Username}!", 36, FontStyle.Bold,
            Color.White, new Point(0, 50), ClientSize.Width, 60, ContentAlignment.TopCenter);

        var sub = MakeLabel("Choose a gaming platform to get started", 16, FontStyle.Regular,
            Color.Gray, new Point(0, 120), ClientSize.Width, 36, ContentAlignment.TopCenter);

        // Enabled launchers from config
        var enabled = _settings.Launchers.Where(l => l.Enabled).ToList();
        int cardW = 260, cardH = 200, gap = 24;
        int cols  = Math.Min(enabled.Count, 4);
        int totalW = cols * cardW + (cols - 1) * gap;
        int startX = (ClientSize.Width - totalW) / 2;
        int startY = 180;

        for (int i = 0; i < enabled.Count; i++)
        {
            var cfg  = enabled[i];
            int col  = i % cols;
            int row  = i / cols;
            int x    = startX + col * (cardW + gap);
            int y    = startY + row * (cardH + gap);

            var bgColor = LauncherColors.TryGetValue(cfg.Key, out var c) ? c : Color.FromArgb(40, 40, 60);
            var card = CreateLauncherCard(cfg, bgColor, new Point(x, y), cardW, cardH);
            bg.Controls.Add(card);
        }

        // Logout link at the bottom
        var logoutBtn = new Button
        {
            Text      = "← Not you? Log out",
            Font      = new Font("Segoe UI", 11),
            Width     = 240, Height = 36,
            Location  = new Point((ClientSize.Width - 240) / 2, ClientSize.Height - 56),
            BackColor = Color.Transparent,
            ForeColor = Color.Gray,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        logoutBtn.FlatAppearance.BorderSize = 0;
        logoutBtn.Click += (s, e) => LogoutAndShowLogin();

        bg.Controls.AddRange(new Control[] { title, sub, logoutBtn });
        Controls.Add(bg);
        ResumeLayout();
    }

    private Panel CreateLauncherCard(LauncherConfig cfg, Color bgColor, Point location, int w, int h)
    {
        var card = new Panel
        {
            Width     = w, Height    = h,
            Location  = location,
            BackColor = bgColor,
            Cursor    = Cursors.Hand
        };

        var icon = new Label
        {
            Text      = cfg.Icon,
            Font      = new Font("Segoe UI Emoji", 36),
            ForeColor = Color.White,
            AutoSize  = false,
            Width     = w, Height   = 70,
            Location  = new Point(0, 28),
            TextAlign = ContentAlignment.TopCenter
        };

        var name = new Label
        {
            Text      = cfg.Name,
            Font      = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize  = false,
            Width     = w, Height   = 36,
            Location  = new Point(0, 102),
            TextAlign = ContentAlignment.TopCenter
        };

        var hint = new Label
        {
            Text      = File.Exists(cfg.ExePath) ? "Ready" : "Click to launch",
            Font      = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(180, 255, 255, 255),
            AutoSize  = false,
            Width     = w, Height   = 26,
            Location  = new Point(0, 144),
            TextAlign = ContentAlignment.TopCenter
        };

        card.Controls.AddRange(new Control[] { icon, name, hint });

        // Hover highlight
        card.MouseEnter += (s, e) => card.BackColor = ControlPaint.Light(bgColor, 0.25f);
        card.MouseLeave += (s, e) => card.BackColor = bgColor;

        // Forward mouse events from child labels to the card click
        EventHandler clickHandler = async (s, e) => await LaunchPlatform(cfg);
        card.Click  += clickHandler;
        icon.Click  += clickHandler;
        name.Click  += clickHandler;
        hint.Click  += clickHandler;

        return card;
    }

    private async Task LaunchPlatform(LauncherConfig cfg)
    {
        try
        {
            if (File.Exists(cfg.ExePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName         = cfg.ExePath,
                    Arguments        = cfg.LaunchArgs,
                    UseShellExecute  = true
                });
            }
            else if (!string.IsNullOrEmpty(cfg.ProtocolUri))
            {
                // Fallback: launch via URI scheme (steam://, com.epicgames.launcher://, etc.)
                Process.Start(new ProcessStartInfo
                {
                    FileName        = cfg.ProtocolUri,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show(
                    $"{cfg.Name} was not found at:\n{cfg.ExePath}\n\nPlease update the path in Settings.",
                    "Launcher Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to launch {cfg.Name}: {ex.Message}",
                "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Update session game name
        if (_currentSession != null)
            _currentSession.GameName = cfg.Name;

        ShowSessionOverlay(cfg.Name);
        await Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────
    // SCREEN 3: HUD widget — shrinks to a small draggable corner overlay
    //           so the launcher (Steam/Epic/etc.) fills the screen behind it.
    // ─────────────────────────────────────────────────────────────────────
    private void ShowSessionOverlay(string platformName)
    {
        SuspendLayout();
        Controls.Clear();

        // ── Shrink form to a small HUD in top-right corner ───────────────
        FormBorderStyle = FormBorderStyle.None;
        TopMost         = true;

        const int HudW = 320, HudH = 200;
        var screen = Screen.PrimaryScreen ?? Screen.AllScreens[0];
        Bounds      = new Rectangle(screen.Bounds.Right - HudW - 16,
                                    screen.Bounds.Top   + 16,
                                    HudW, HudH);
        WindowState = FormWindowState.Normal;

        BackColor   = Color.FromArgb(18, 22, 32);

        // Drag support: click anywhere on the HUD to move it
        Point _dragStart = Point.Empty;
        MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) _dragStart = e.Location; };
        MouseMove += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
                Location = new Point(Location.X + e.X - _dragStart.X,
                                     Location.Y + e.Y - _dragStart.Y);
        };

        // ── User + platform row ──────────────────────────────────────────
        var userLbl = MakeLabel($"👤 {_currentUser?.Username}   🎮 {platformName}",
            9, FontStyle.Regular, Color.FromArgb(160, 200, 255),
            new Point(10, 10), HudW - 20, 20);

        // ── Timer ────────────────────────────────────────────────────────
        var timerLabel = MakeLabel("00:00:00",
            36, FontStyle.Bold, Color.LimeGreen,
            new Point(0, 30), HudW, 52, ContentAlignment.TopCenter);

        // ── Cost ─────────────────────────────────────────────────────────
        var costLabel = MakeLabel("$0.00",
            22, FontStyle.Bold, Color.Yellow,
            new Point(0, 82), HudW, 36, ContentAlignment.TopCenter);

        // ── Buttons row ──────────────────────────────────────────────────
        var switchBtn = new Button
        {
            Text      = "⬅ Switch",
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            Width     = 100, Height = 32,
            Location  = new Point(10, HudH - 44),
            BackColor = Color.FromArgb(50, 90, 160),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        switchBtn.FlatAppearance.BorderSize = 0;
        switchBtn.Click += (s, e) => ShowLauncherSelectionAsFullscreen();

        var endBtn = new Button
        {
            Text      = "End Session",
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            Width     = 120, Height = 32,
            Location  = new Point(HudW - 130, HudH - 44),
            BackColor = Color.FromArgb(180, 30, 30),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        endBtn.FlatAppearance.BorderSize = 0;
        endBtn.Click += (s, e) => LogoutAndShowLogin();

        var hintLbl = MakeLabel("drag to move  •  Ctrl+Shift+Q to end",
            7, FontStyle.Regular, Color.FromArgb(80, 120, 80),
            new Point(0, HudH - 14), HudW, 14, ContentAlignment.TopCenter);

        Controls.AddRange(new Control[] { userLbl, timerLabel, costLabel, switchBtn, endBtn, hintLbl });
        ResumeLayout();

        StartSessionTimer(timerLabel, costLabel);
    }

    // Returns to full-screen launcher selection (pauses the game choice)
    private void ShowLauncherSelectionAsFullscreen()
    {
        StopSessionTimer();
        var screen = Screen.PrimaryScreen ?? Screen.AllScreens[0];
        Bounds      = screen.Bounds;
        WindowState = FormWindowState.Maximized;
        ShowLauncherSelection();
    }

    // ─────────────────────────────────────────────────────────────────────
    // Timer
    // ─────────────────────────────────────────────────────────────────────
    private void StartSessionTimer(Label timerLabel, Label costLabel)
    {
        StopSessionTimer();
        _sessionTimer          = new System.Windows.Forms.Timer { Interval = 1000 };
        _sessionTimer.Tick    += (s, e) =>
        {
            if (_currentSession == null) return;
            var elapsed     = DateTime.UtcNow - _currentSession.StartTime;
            timerLabel.Text = elapsed.ToString(@"hh\:mm\:ss");
            var cost        = _billingService.CalculateHourlyCost(_settings.HourlyRate, (int)elapsed.TotalMinutes);
            costLabel.Text  = $"${cost:F2}";
        };
        _sessionTimer.Start();
    }

    private void StopSessionTimer()
    {
        _sessionTimer?.Stop();
        _sessionTimer?.Dispose();
        _sessionTimer = null;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Logout / session end
    // ─────────────────────────────────────────────────────────────────────
    private void LogoutAndShowLogin()
    {
        StopSessionTimer();

        if (_currentSession != null)
        {
            try
            {
                var elapsed     = DateTime.UtcNow - _currentSession.StartTime;
                int minutes     = (int)elapsed.TotalMinutes;
                decimal cost    = _billingService.CalculateHourlyCost(_settings.HourlyRate, minutes);

                _sessionService.EndSessionAsync(_currentSession.Id).Wait();

                MessageBox.Show(
                    $"Session Ended\n\nDuration: {elapsed:hh\\:mm\\:ss}\nTotal Cost: ${cost:F2}",
                    "Billing Summary", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch { }

            _currentSession = null;
        }

        _currentUser = null;

        // Kill any known launchers
        foreach (var name in new[] { "Playnite", "steam", "EpicGamesLauncher", "EADesktop", "Origin" })
        {
            try
            {
                foreach (var p in Process.GetProcessesByName(name))
                {
                    p.CloseMainWindow();
                    if (!p.WaitForExit(2000)) p.Kill();
                }
            }
            catch { }
        }

        ShowLoginScreen();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        StopSessionTimer();
        base.OnFormClosing(e);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────
    private static Label MakeLabel(string text, float size, FontStyle style, Color color,
        Point loc, int width, int height, ContentAlignment align = ContentAlignment.TopLeft)
        => new Label
        {
            Text      = text,
            Font      = new Font("Segoe UI", size, style),
            ForeColor = color,
            Location  = loc,
            Width     = width, Height = height,
            TextAlign = align,
            AutoSize  = false
        };

    private static TextBox MakeTextBox(Point loc, int width, bool password)
        => new TextBox
        {
            Font         = new Font("Segoe UI", 14),
            Width        = width, Height = 40,
            Location     = loc,
            BackColor    = Color.FromArgb(40, 44, 60),
            ForeColor    = Color.White,
            BorderStyle  = BorderStyle.FixedSingle,
            PasswordChar = password ? '*' : '\0'
        };
}
