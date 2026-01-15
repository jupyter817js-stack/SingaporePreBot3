
namespace SingaporePreBot3
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.pnlFormLoader = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panNav = new System.Windows.Forms.Panel();
            this.btnSetting = new FontAwesome.Sharp.IconButton();
            this.btnBlackLeague = new FontAwesome.Sharp.IconButton();
            this.btnHistory = new FontAwesome.Sharp.IconButton();
            this.btnArb = new FontAwesome.Sharp.IconButton();
            this.btnSuperodd = new FontAwesome.Sharp.IconButton();
            this.btnSingapore = new FontAwesome.Sharp.IconButton();
            this.btnWork = new FontAwesome.Sharp.IconButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblUsername = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.btnMinimize = new FontAwesome.Sharp.IconButton();
            this.btnClose = new FontAwesome.Sharp.IconButton();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlFormLoader
            // 
            this.pnlFormLoader.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlFormLoader.Location = new System.Drawing.Point(166, 57);
            this.pnlFormLoader.Name = "pnlFormLoader";
            this.pnlFormLoader.Size = new System.Drawing.Size(1160, 730);
            this.pnlFormLoader.TabIndex = 9;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(30)))), ((int)(((byte)(54)))));
            this.panel1.Controls.Add(this.panNav);
            this.panel1.Controls.Add(this.btnSetting);
            this.panel1.Controls.Add(this.btnBlackLeague);
            this.panel1.Controls.Add(this.btnHistory);
            this.panel1.Controls.Add(this.btnArb);
            this.panel1.Controls.Add(this.btnSuperodd);
            this.panel1.Controls.Add(this.btnSingapore);
            this.panel1.Controls.Add(this.btnWork);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(152, 800);
            this.panel1.TabIndex = 7;
            // 
            // panNav
            // 
            this.panNav.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.panNav.Location = new System.Drawing.Point(0, 161);
            this.panNav.Name = "panNav";
            this.panNav.Size = new System.Drawing.Size(3, 100);
            this.panNav.TabIndex = 7;
            // 
            // btnSetting
            // 
            this.btnSetting.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(30)))), ((int)(((byte)(54)))));
            this.btnSetting.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnSetting.FlatAppearance.BorderSize = 0;
            this.btnSetting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetting.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnSetting.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnSetting.IconChar = FontAwesome.Sharp.IconChar.Gears;
            this.btnSetting.IconColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnSetting.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.btnSetting.IconSize = 30;
            this.btnSetting.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSetting.Location = new System.Drawing.Point(0, 392);
            this.btnSetting.Name = "btnSetting";
            this.btnSetting.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnSetting.Size = new System.Drawing.Size(152, 40);
            this.btnSetting.TabIndex = 13;
            this.btnSetting.Text = "Setting";
            this.btnSetting.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSetting.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSetting.UseVisualStyleBackColor = false;
            this.btnSetting.Click += new System.EventHandler(this.button_Click);
            // 
            // btnBlackLeague
            // 
            this.btnBlackLeague.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(30)))), ((int)(((byte)(54)))));
            this.btnBlackLeague.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnBlackLeague.FlatAppearance.BorderSize = 0;
            this.btnBlackLeague.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBlackLeague.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnBlackLeague.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnBlackLeague.IconChar = FontAwesome.Sharp.IconChar.ThumbsDown;
            this.btnBlackLeague.IconColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnBlackLeague.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.btnBlackLeague.IconSize = 30;
            this.btnBlackLeague.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnBlackLeague.Location = new System.Drawing.Point(0, 352);
            this.btnBlackLeague.Name = "btnBlackLeague";
            this.btnBlackLeague.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnBlackLeague.Size = new System.Drawing.Size(152, 40);
            this.btnBlackLeague.TabIndex = 18;
            this.btnBlackLeague.Tag = "Black";
            this.btnBlackLeague.Text = "Black";
            this.btnBlackLeague.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnBlackLeague.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnBlackLeague.UseVisualStyleBackColor = false;
            this.btnBlackLeague.Click += new System.EventHandler(this.button_Click);
            // 
            // btnHistory
            // 
            this.btnHistory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(30)))), ((int)(((byte)(54)))));
            this.btnHistory.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnHistory.FlatAppearance.BorderSize = 0;
            this.btnHistory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHistory.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnHistory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnHistory.IconChar = FontAwesome.Sharp.IconChar.ClockRotateLeft;
            this.btnHistory.IconColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnHistory.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.btnHistory.IconSize = 30;
            this.btnHistory.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnHistory.Location = new System.Drawing.Point(0, 312);
            this.btnHistory.Name = "btnHistory";
            this.btnHistory.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnHistory.Size = new System.Drawing.Size(152, 40);
            this.btnHistory.TabIndex = 12;
            this.btnHistory.Text = "History";
            this.btnHistory.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnHistory.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnHistory.UseVisualStyleBackColor = false;
            this.btnHistory.Visible = false;
            this.btnHistory.Click += new System.EventHandler(this.button_Click);
            // 
            // btnArb
            // 
            this.btnArb.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(30)))), ((int)(((byte)(54)))));
            this.btnArb.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnArb.FlatAppearance.BorderSize = 0;
            this.btnArb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnArb.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnArb.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnArb.IconChar = FontAwesome.Sharp.IconChar.ClipboardCheck;
            this.btnArb.IconColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnArb.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.btnArb.IconSize = 30;
            this.btnArb.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnArb.Location = new System.Drawing.Point(0, 272);
            this.btnArb.Name = "btnArb";
            this.btnArb.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnArb.Size = new System.Drawing.Size(152, 40);
            this.btnArb.TabIndex = 17;
            this.btnArb.Text = "Arb";
            this.btnArb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnArb.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnArb.UseVisualStyleBackColor = false;
            this.btnArb.Visible = false;
            this.btnArb.Click += new System.EventHandler(this.button_Click);
            // 
            // btnSuperodd
            // 
            this.btnSuperodd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(30)))), ((int)(((byte)(54)))));
            this.btnSuperodd.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnSuperodd.FlatAppearance.BorderSize = 0;
            this.btnSuperodd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSuperodd.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnSuperodd.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnSuperodd.IconChar = FontAwesome.Sharp.IconChar.Map;
            this.btnSuperodd.IconColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnSuperodd.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.btnSuperodd.IconSize = 30;
            this.btnSuperodd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSuperodd.Location = new System.Drawing.Point(0, 232);
            this.btnSuperodd.Name = "btnSuperodd";
            this.btnSuperodd.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnSuperodd.Size = new System.Drawing.Size(152, 40);
            this.btnSuperodd.TabIndex = 15;
            this.btnSuperodd.Text = "Colourhe";
            this.btnSuperodd.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSuperodd.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSuperodd.UseVisualStyleBackColor = false;
            this.btnSuperodd.Click += new System.EventHandler(this.button_Click);
            // 
            // btnSingapore
            // 
            this.btnSingapore.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(30)))), ((int)(((byte)(54)))));
            this.btnSingapore.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnSingapore.FlatAppearance.BorderSize = 0;
            this.btnSingapore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSingapore.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnSingapore.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnSingapore.IconChar = FontAwesome.Sharp.IconChar.Award;
            this.btnSingapore.IconColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnSingapore.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.btnSingapore.IconSize = 30;
            this.btnSingapore.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSingapore.Location = new System.Drawing.Point(0, 192);
            this.btnSingapore.Name = "btnSingapore";
            this.btnSingapore.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnSingapore.Size = new System.Drawing.Size(152, 40);
            this.btnSingapore.TabIndex = 14;
            this.btnSingapore.Text = "Singapore";
            this.btnSingapore.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSingapore.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSingapore.UseVisualStyleBackColor = false;
            this.btnSingapore.Visible = false;
            this.btnSingapore.Click += new System.EventHandler(this.button_Click);
            // 
            // btnWork
            // 
            this.btnWork.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(30)))), ((int)(((byte)(54)))));
            this.btnWork.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnWork.FlatAppearance.BorderSize = 0;
            this.btnWork.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWork.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.btnWork.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnWork.IconChar = FontAwesome.Sharp.IconChar.House;
            this.btnWork.IconColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnWork.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.btnWork.IconSize = 30;
            this.btnWork.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnWork.Location = new System.Drawing.Point(0, 152);
            this.btnWork.Name = "btnWork";
            this.btnWork.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnWork.Size = new System.Drawing.Size(152, 40);
            this.btnWork.TabIndex = 8;
            this.btnWork.Tag = "";
            this.btnWork.Text = "Main";
            this.btnWork.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnWork.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnWork.UseVisualStyleBackColor = false;
            this.btnWork.Click += new System.EventHandler(this.button_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lblUsername);
            this.panel2.Controls.Add(this.pictureBox1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(152, 152);
            this.panel2.TabIndex = 1;
            this.panel2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.frmMain_MouseMove);
            // 
            // lblUsername
            // 
            this.lblUsername.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lblUsername.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold);
            this.lblUsername.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.lblUsername.Location = new System.Drawing.Point(1, 109);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(151, 20);
            this.lblUsername.TabIndex = 2;
            this.lblUsername.Text = "User Name";
            this.lblUsername.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblUsername.Click += new System.EventHandler(this.lblUsername_Click);
            this.lblUsername.MouseMove += new System.Windows.Forms.MouseEventHandler(this.frmMain_MouseMove);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::SingaporePreBot3.Properties.Resources.crown;
            this.pictureBox1.Location = new System.Drawing.Point(21, 15);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(110, 91);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.frmMain_MouseMove);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(161)))), ((int)(((byte)(176)))));
            this.lblTitle.Location = new System.Drawing.Point(161, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(140, 29);
            this.lblTitle.TabIndex = 8;
            this.lblTitle.Text = "Dashboard";
            this.lblTitle.MouseMove += new System.Windows.Forms.MouseEventHandler(this.frmMain_MouseMove);
            // 
            // timer
            // 
            this.timer.Enabled = true;
            this.timer.Interval = 1000;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // btnMinimize
            // 
            this.btnMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMinimize.FlatAppearance.BorderSize = 0;
            this.btnMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMinimize.IconChar = FontAwesome.Sharp.IconChar.WindowMinimize;
            this.btnMinimize.IconColor = System.Drawing.Color.White;
            this.btnMinimize.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.btnMinimize.IconSize = 30;
            this.btnMinimize.Location = new System.Drawing.Point(1269, 10);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(25, 22);
            this.btnMinimize.TabIndex = 11;
            this.btnMinimize.UseVisualStyleBackColor = true;
            this.btnMinimize.Click += new System.EventHandler(this.btnMinimize_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.IconChar = FontAwesome.Sharp.IconChar.Xmark;
            this.btnClose.IconColor = System.Drawing.Color.White;
            this.btnClose.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.btnClose.IconSize = 30;
            this.btnClose.Location = new System.Drawing.Point(1300, 10);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(25, 22);
            this.btnClose.TabIndex = 10;
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // frmMain
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(51)))), ((int)(((byte)(73)))));
            this.ClientSize = new System.Drawing.Size(1340, 800);
            this.Controls.Add(this.pnlFormLoader);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnMinimize);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmMain";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.frmMain_MouseMove);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlFormLoader;
        private System.Windows.Forms.Panel panel1;
        private FontAwesome.Sharp.IconButton btnSetting;
        private System.Windows.Forms.Panel panNav;
        private FontAwesome.Sharp.IconButton btnHistory;
        private FontAwesome.Sharp.IconButton btnSingapore;
        private FontAwesome.Sharp.IconButton btnWork;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Timer timer;
        private FontAwesome.Sharp.IconButton btnMinimize;
        private FontAwesome.Sharp.IconButton btnClose;
        private FontAwesome.Sharp.IconButton btnSuperodd;
        private FontAwesome.Sharp.IconButton btnArb;
        private FontAwesome.Sharp.IconButton btnBlackLeague;
    }
}