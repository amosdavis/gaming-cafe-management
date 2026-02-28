using GameCafe.Core.Models;
using GameCafe.Core.Security;
using GameCafe.Core.Services;

namespace GameCafe.StationAgent;

public partial class MainForm : Form
{
    private readonly IAuthenticationService _authService;
    private readonly ISessionService _sessionService;
    private readonly IPlayniteIntegrationService _playniteService;
    private readonly IBillingService _billingService;
    
    private User? _currentUser;
    private Session? _currentSession;
    private System.Windows.Forms.Timer? _sessionTimer;

    public MainForm()
    {
        InitializeComponent();
        
        // Initialize services
        var passwordHasher = new PasswordHasher();
        _authService = new AuthenticationService(passwordHasher);
        _sessionService = new SessionService();
        _playniteService = new PlayniteIntegrationService();
        _billingService = new BillingService();
        
        // Subscribe to session events
        _sessionService.SessionStarted += OnSessionStarted;
        _sessionService.SessionEnded += OnSessionEnded;
    }

    private void OnSessionStarted(object? sender, SessionEventArgs e)
    {
        this.Invoke(() =>
        {
            lblStatus.Text = $"Session Started: {e.Session.GameName}";
            lblStatus.ForeColor = Color.Green;
            StartSessionTimer();
        });
    }

    private void OnSessionEnded(object? sender, SessionEventArgs e)
    {
        this.Invoke(() =>
        {
            lblStatus.Text = $"Session Ended - Duration: {e.Session.DurationMinutes} min";
            lblStatus.ForeColor = Color.Red;
            StopSessionTimer();
        });
    }

    private void StartSessionTimer()
    {
        _sessionTimer = new System.Windows.Forms.Timer();
        _sessionTimer.Interval = 1000; // Update every second
        _sessionTimer.Tick += (s, e) => UpdateSessionDisplay();
        _sessionTimer.Start();
    }

    private void StopSessionTimer()
    {
        _sessionTimer?.Stop();
        _sessionTimer?.Dispose();
        _sessionTimer = null;
    }

    private void UpdateSessionDisplay()
    {
        if (_currentSession != null)
        {
            var duration = _currentSession.DurationMinutes;
            var cost = _billingService.CalculateHourlyCost(5.00m, duration); // Default $5/hr
            
            lblDuration.Text = $"Duration: {duration} min";
            lblCost.Text = $"Cost: ${cost:F2}";
        }
    }

    private async void btnLogin_Click(object sender, EventArgs e)
    {
        var username = txtUsername.Text;
        var password = txtPassword.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Please enter username and password");
            return;
        }

        var result = await _authService.LoginAsync(username, password);
        if (result.Success && result.User != null)
        {
            _currentUser = result.User;
            lblStatus.Text = $"Logged in as: {_currentUser.Username}";
            lblStatus.ForeColor = Color.Green;
            pnlLogin.Visible = false;
            pnlSession.Visible = true;
        }
        else
        {
            MessageBox.Show("Login failed");
        }
    }

    private async void btnStartGame_Click(object sender, EventArgs e)
    {
        if (_currentUser == null)
        {
            MessageBox.Show("Please login first");
            return;
        }

        var gameName = txtGameName.Text;
        if (string.IsNullOrWhiteSpace(gameName))
        {
            MessageBox.Show("Please enter a game name");
            return;
        }

        // Create session
        _currentSession = await _sessionService.CreateSessionAsync(_currentUser.Id, 1, gameName);
        
        // Launch game via Playnite
        var launched = await _playniteService.LaunchGameAsync(gameName);
        if (!launched)
        {
            MessageBox.Show("Failed to launch game");
            await _sessionService.EndSessionAsync(_currentSession.Id);
        }
    }

    private async void btnEndSession_Click(object sender, EventArgs e)
    {
        if (_currentSession != null)
        {
            await _sessionService.EndSessionAsync(_currentSession.Id);
            var cost = _billingService.CalculateHourlyCost(5.00m, _currentSession.DurationMinutes);
            MessageBox.Show($"Session ended. Total cost: ${cost:F2}");
            _currentSession = null;
        }
    }

    private async void btnLogout_Click(object sender, EventArgs e)
    {
        if (_currentSession != null)
        {
            await _sessionService.EndSessionAsync(_currentSession.Id);
        }
        _currentUser = null;
        pnlLogin.Visible = true;
        pnlSession.Visible = false;
        lblStatus.Text = "Logged out";
        lblStatus.ForeColor = Color.Black;
    }
}
