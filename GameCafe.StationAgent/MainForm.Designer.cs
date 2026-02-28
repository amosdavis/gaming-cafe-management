namespace GameCafe.StationAgent;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        this.Text = "Gaming Cafe Station";
        this.Size = new System.Drawing.Size(600, 500);
        this.StartPosition = FormStartPosition.CenterScreen;

        // Login Panel
        pnlLogin = new Panel { Dock = DockStyle.Fill, BackColor = SystemColors.Control };
        
        var lblUsernameLabel = new Label { Text = "Username:", Location = new System.Drawing.Point(20, 20), Size = new System.Drawing.Size(100, 20) };
        txtUsername = new TextBox { Location = new System.Drawing.Point(130, 20), Size = new System.Drawing.Size(300, 25) };
        
        var lblPasswordLabel = new Label { Text = "Password:", Location = new System.Drawing.Point(20, 60), Size = new System.Drawing.Size(100, 20) };
        txtPassword = new TextBox { Location = new System.Drawing.Point(130, 60), Size = new System.Drawing.Size(300, 25), PasswordChar = '*' };
        
        btnLogin = new Button { Text = "Login", Location = new System.Drawing.Point(130, 100), Size = new System.Drawing.Size(100, 35) };
        btnLogin.Click += btnLogin_Click;

        pnlLogin.Controls.AddRange(new Control[] { lblUsernameLabel, txtUsername, lblPasswordLabel, txtPassword, btnLogin });

        // Session Panel
        pnlSession = new Panel { Dock = DockStyle.Fill, BackColor = SystemColors.Control, Visible = false };
        
        lblStatus = new Label { Text = "Session Active", Location = new System.Drawing.Point(20, 20), Size = new System.Drawing.Size(400, 20), Font = new Font("Arial", 12, FontStyle.Bold) };
        
        var lblGameLabel = new Label { Text = "Game Name:", Location = new System.Drawing.Point(20, 60), Size = new System.Drawing.Size(100, 20) };
        txtGameName = new TextBox { Location = new System.Drawing.Point(130, 60), Size = new System.Drawing.Size(300, 25) };
        
        btnStartGame = new Button { Text = "Start Game", Location = new System.Drawing.Point(130, 100), Size = new System.Drawing.Size(100, 35) };
        btnStartGame.Click += btnStartGame_Click;
        
        lblDuration = new Label { Text = "Duration: 0 min", Location = new System.Drawing.Point(20, 150), Size = new System.Drawing.Size(200, 20) };
        lblCost = new Label { Text = "Cost: $0.00", Location = new System.Drawing.Point(20, 180), Size = new System.Drawing.Size(200, 20) };
        
        btnEndSession = new Button { Text = "End Session", Location = new System.Drawing.Point(130, 220), Size = new System.Drawing.Size(100, 35), BackColor = Color.LightCoral };
        btnEndSession.Click += btnEndSession_Click;
        
        btnLogout = new Button { Text = "Logout", Location = new System.Drawing.Point(250, 220), Size = new System.Drawing.Size(100, 35) };
        btnLogout.Click += btnLogout_Click;

        pnlSession.Controls.AddRange(new Control[] { lblStatus, lblGameLabel, txtGameName, btnStartGame, lblDuration, lblCost, btnEndSession, btnLogout });

        this.Controls.AddRange(new Control[] { pnlLogin, pnlSession });
    }

    private Panel pnlLogin = null!;
    private TextBox txtUsername = null!;
    private TextBox txtPassword = null!;
    private Button btnLogin = null!;
    
    private Panel pnlSession = null!;
    private Label lblStatus = null!;
    private TextBox txtGameName = null!;
    private Button btnStartGame = null!;
    private Label lblDuration = null!;
    private Label lblCost = null!;
    private Button btnEndSession = null!;
    private Button btnLogout = null!;
}
