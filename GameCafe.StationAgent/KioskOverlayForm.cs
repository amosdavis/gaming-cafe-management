using System.Diagnostics;
using GameCafe.Core.Models;
using GameCafe.Core.Security;
using GameCafe.Core.Services;

namespace GameCafe.StationAgent;

/// <summary>
/// Full-screen overlay for gaming cafe kiosk mode.
/// Displays on top of Playnite with session timer and billing information.
/// </summary>
public class KioskOverlayForm : Form
{
    private readonly IAuthenticationService _authService;
    private readonly ISessionService _sessionService;
    private readonly IPlayniteIntegrationService _playniteService;
    private readonly IBillingService _billingService;

    private User? _currentUser;
    private Session? _currentSession;
    private System.Windows.Forms.Timer? _sessionTimer;
    private int _stationId = 1; // Default station ID

    public KioskOverlayForm()
    {
        // Initialize services
        var passwordHasher = new PasswordHasher();
        _authService = new AuthenticationService(passwordHasher);
        _sessionService = new SessionService();
        _playniteService = new PlayniteIntegrationService();
        _billingService = new BillingService();

        // Setup fullscreen kiosk mode
        this.FormBorderStyle = FormBorderStyle.None;
        this.ControlBox = false;
        this.KeyPreview = true;
        this.Text = "Gaming Cafe Kiosk";
        this.BackColor = Color.Black;
        this.StartPosition = FormStartPosition.Manual;
        this.KeyDown += KioskOverlayForm_KeyDown;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        
        // Force fullscreen AFTER form is loaded
        var screen = Screen.PrimaryScreen ?? Screen.AllScreens[0];
        this.Bounds = screen.Bounds;
        this.WindowState = FormWindowState.Maximized;
        this.TopMost = true;
        
        ShowLoginScreen();
    }

    private void KioskOverlayForm_KeyDown(object? sender, KeyEventArgs e)
    {
        // Allow Ctrl+Shift+Q for admin logout
        if (e.Control && e.Shift && e.KeyCode == Keys.Q)
        {
            e.Handled = true;
            LogoutAndShowLogin();
        }
    }

    private void ShowLoginScreen()
    {
        this.SuspendLayout();
        this.Controls.Clear();

        // Login panel
        Panel loginPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(20, 20, 30)
        };

        // Title
        Label titleLabel = new Label
        {
            Text = "🎮 GAMING CAFE",
            Font = new Font("Arial", 48, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.TopCenter,
            Location = new Point(0, 80),
            Width = this.ClientSize.Width,
            Height = 80,
            AutoSize = false
        };

        // Username input
        Label userLabel = new Label
        {
            Text = "Username:",
            Font = new Font("Arial", 14),
            ForeColor = Color.White,
            Location = new Point((this.ClientSize.Width - 300) / 2, 250),
            AutoSize = true
        };

        TextBox userTextBox = new TextBox
        {
            Font = new Font("Arial", 14),
            Width = 300,
            Height = 40,
            Location = new Point((this.ClientSize.Width - 300) / 2, 280),
            BackColor = Color.White,
            ForeColor = Color.Black
        };

        // Password input
        Label passLabel = new Label
        {
            Text = "Password:",
            Font = new Font("Arial", 14),
            ForeColor = Color.White,
            Location = new Point((this.ClientSize.Width - 300) / 2, 330),
            AutoSize = true
        };

        TextBox passTextBox = new TextBox
        {
            Font = new Font("Arial", 14),
            Width = 300,
            Height = 40,
            Location = new Point((this.ClientSize.Width - 300) / 2, 360),
            PasswordChar = '*',
            BackColor = Color.White,
            ForeColor = Color.Black
        };

        // Login button
        Button loginButton = new Button
        {
            Text = "LOGIN",
            Font = new Font("Arial", 16, FontStyle.Bold),
            Width = 300,
            Height = 50,
            Location = new Point((this.ClientSize.Width - 300) / 2, 430),
            BackColor = Color.FromArgb(0, 150, 255),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };

        loginButton.Click += async (s, e) =>
        {
            await HandleLogin(userTextBox.Text, passTextBox.Text);
        };

        // Status label
        Label statusLabel = new Label
        {
            Text = "Enter your credentials to continue",
            Font = new Font("Arial", 12),
            ForeColor = Color.Gray,
            Location = new Point(0, this.ClientSize.Height - 50),
            Width = this.ClientSize.Width,
            Height = 50,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false
        };

        loginPanel.Controls.AddRange(new Control[] 
        { 
            titleLabel, 
            userLabel, 
            userTextBox, 
            passLabel, 
            passTextBox, 
            loginButton, 
            statusLabel 
        });

        this.Controls.Add(loginPanel);
        this.ResumeLayout();
        userTextBox.Focus();
    }

    private async Task HandleLogin(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Please enter username and password.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            // Authenticate user
            var authResult = await _authService.LoginAsync(username, password);
            if (!authResult.Success || authResult.User == null)
            {
                MessageBox.Show($"Authentication failed: {authResult.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _currentUser = authResult.User;

            // Create session (assuming userId for station)
            _currentSession = await _sessionService.CreateSessionAsync(_currentUser.Id, _stationId, "Playnite Library");
            
            // Launch Playnite fullscreen
            await _playniteService.StartPlayniteKioskAsync();

            ShowSessionOverlay();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Authentication failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowSessionOverlay()
    {
        this.SuspendLayout();
        this.Controls.Clear();

        // Semi-transparent overlay
        Panel overlayPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(30, 0, 0, 0)
        };

        // Top-left: User info
        Panel topLeftPanel = new Panel
        {
            Width = 300,
            Height = 120,
            Location = new Point(20, 20),
            BackColor = Color.FromArgb(200, 20, 20, 30),
            BorderStyle = BorderStyle.FixedSingle
        };

        Label userInfoLabel = new Label
        {
            Text = $"👤 {_currentUser?.Username}\n\nID: {_currentUser?.Id}",
            Font = new Font("Arial", 12),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(10, 10)
        };
        topLeftPanel.Controls.Add(userInfoLabel);

        // Top-right: Timer and Cost (large)
        Panel topRightPanel = new Panel
        {
            Width = 400,
            Height = 150,
            Location = new Point(this.ClientSize.Width - 420, 20),
            BackColor = Color.FromArgb(200, 30, 100, 30),
            BorderStyle = BorderStyle.FixedSingle
        };

        Label timerLabel = new Label
        {
            Text = "00:00:00",
            Font = new Font("Arial", 48, FontStyle.Bold),
            ForeColor = Color.LimeGreen,
            TextAlign = ContentAlignment.TopCenter,
            Location = new Point(0, 10),
            Width = 400,
            Height = 70,
            AutoSize = false
        };

        Label costLabel = new Label
        {
            Text = "$0.00",
            Font = new Font("Arial", 36, FontStyle.Bold),
            ForeColor = Color.Yellow,
            TextAlign = ContentAlignment.BottomCenter,
            Location = new Point(0, 70),
            Width = 400,
            Height = 70,
            AutoSize = false
        };

        topRightPanel.Controls.AddRange(new Control[] { timerLabel, costLabel });

        // Bottom: Logout button
        Button logoutButton = new Button
        {
            Text = "END SESSION (Ctrl+Shift+Q)",
            Font = new Font("Arial", 14, FontStyle.Bold),
            Width = 350,
            Height = 60,
            Location = new Point((this.ClientSize.Width - 350) / 2, this.ClientSize.Height - 80),
            BackColor = Color.FromArgb(220, 0, 0),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };

        logoutButton.Click += (s, e) => LogoutAndShowLogin();

        overlayPanel.Controls.AddRange(new Control[] 
        { 
            topLeftPanel, 
            topRightPanel, 
            logoutButton 
        });

        this.Controls.Add(overlayPanel);
        this.ResumeLayout();

        // Start session timer (updates every 1 second)
        StartSessionTimer(timerLabel, costLabel);
    }

    private void StartSessionTimer(Label timerLabel, Label costLabel)
    {
        _sessionTimer = new System.Windows.Forms.Timer();
        _sessionTimer.Interval = 1000; // Update every 1 second
        _sessionTimer.Tick += (s, e) =>
        {
            if (_currentSession != null)
            {
                var elapsed = DateTime.UtcNow - _currentSession.StartTime;
                timerLabel.Text = elapsed.ToString(@"hh\:mm\:ss");

                // Calculate cost - duration in decimal minutes
                int elapsedMinutes = (int)elapsed.TotalMinutes;
                decimal cost = _billingService.CalculateHourlyCost(0, elapsedMinutes);
                costLabel.Text = $"${cost:F2}";
            }
        };
        _sessionTimer.Start();
    }

    private void StopSessionTimer()
    {
        if (_sessionTimer != null)
        {
            _sessionTimer.Stop();
            _sessionTimer.Dispose();
            _sessionTimer = null;
        }
    }

    private void LogoutAndShowLogin()
    {
        StopSessionTimer();

        if (_currentSession != null)
        {
            try
            {
                // Calculate final billing
                var elapsed = DateTime.UtcNow - _currentSession.StartTime;
                int elapsedMinutes = (int)elapsed.TotalMinutes;
                decimal finalCost = _billingService.CalculateHourlyCost(0, elapsedMinutes);

                // End the session
                _sessionService.EndSessionAsync(_currentSession.Id).Wait();

                // Show billing summary
                MessageBox.Show(
                    $"Session Ended\n\nDuration: {elapsedMinutes} minutes\nTotal Cost: ${finalCost:F2}",
                    "Billing Summary",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch { }

            _currentSession = null;
        }

        _currentUser = null;

        // Close Playnite
        try
        {
            var processes = Process.GetProcessesByName("Playnite");
            foreach (var proc in processes)
            {
                proc.CloseMainWindow();
                proc.WaitForExit(2000);
                if (!proc.HasExited)
                    proc.Kill();
            }
        }
        catch { }

        ShowLoginScreen();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        StopSessionTimer();
        base.OnFormClosing(e);
    }
}
