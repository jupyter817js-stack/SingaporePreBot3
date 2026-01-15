using FontAwesome.Sharp;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SingaporePreBot3
{
    public partial class frmMain : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        frmWork dlgWork = null;
        frmSingapore dlgSingapore = null;
        frmSuperodd dlgSuperodd = null;
        frmHistory dlgHistory = null;
        frmSetting dlgSetting = null;
        frmArb dlgArb = null;
        frmBlack dlgBlack = null;
        private const int cGrip = 16;      // Grip size
        private const int cCaption = 32;   // Caption bar height;
        private int tickCount;
        public frmMain()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 15, 15));
            panNav.Height = btnWork.Height;
            panNav.Top = btnWork.Top;
            panNav.Left = btnWork.Left;
            btnWork.BackColor = Color.FromArgb(46, 51, 73);
            pnlFormLoader.Controls.Clear();
            //frmDashboard dashboardDlg = new frmDashboard() { Dock = DockStyle.Fill, TopLevel = false, TopMost = true };
            //dashboardDlg.FormBorderStyle = FormBorderStyle.None;
            Setting.Instance.load();
            dlgWork = new frmWork() { Dock = DockStyle.Fill, TopLevel = false };
            dlgSingapore = new frmSingapore() { Dock = DockStyle.Fill, TopLevel = false };
            dlgSuperodd = new frmSuperodd() { Dock = DockStyle.Fill, TopLevel = false };
            dlgHistory = new frmHistory() { Dock = DockStyle.Fill, TopLevel = false };
            dlgSetting = new frmSetting() { Dock = DockStyle.Fill, TopLevel = false };
            dlgArb = new frmArb() { Dock = DockStyle.Fill, TopLevel = false };
            dlgBlack = new frmBlack() { Dock = DockStyle.Fill, TopLevel = false };
            pnlFormLoader.Controls.Add(dlgWork);
            pnlFormLoader.Controls.Add(dlgSingapore);
            pnlFormLoader.Controls.Add(dlgSuperodd);
            pnlFormLoader.Controls.Add(dlgArb);
            pnlFormLoader.Controls.Add(dlgHistory);
            pnlFormLoader.Controls.Add(dlgSetting);
            pnlFormLoader.Controls.Add(dlgBlack);
            dlgWork.Show();
            Global.WriteStatus += dlgWork.writeStatus;
            Global.StopAutomation += dlgWork.stopAutomation;
            Global.BetFinish += dlgHistory.updateOrderList;
            Global.BalanceUpdate += dlgWork.updateStatus;
            Global.UpdateSingapore += dlgSingapore.updateMatch;
            Global.UpdateSuperodd += dlgSuperodd.updateMatch;
            Global.GetCookie += Setting.Instance.browser.getCookie;
            Global.RefreshBrowser += Setting.Instance.browser.refreshBrowser;
            Global.KeepLogin += Setting.Instance.browser.keepLogin;
            Global.UpdateArb += dlgArb.updateArb;
            Global.GetAuthToken += Setting.Instance.browser.getAuthToken;
            Setting.Instance.chromePath = getChromePath();
            //killProcessByPort();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 15, 15));
        }
        
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84)
            {  // Trap WM_NCHITTEST
                Point pos = new Point(m.LParam.ToInt32());
                pos = this.PointToClient(pos);
                if (pos.Y < cCaption)
                {
                    m.Result = (IntPtr)2;  // HTCAPTION
                    return;
                }
                if (pos.X >= this.ClientSize.Width - cGrip && pos.Y >= this.ClientSize.Height - cGrip)
                {
                    m.Result = (IntPtr)17; // HTBOTTOMRIGHT
                    return;
                }
            }
            base.WndProc(ref m);
        }
        private void frmMain_Load(object sender, EventArgs e)
        {
            lblTitle.Text = $"SingaporePreBot 3 Prematch Bot - {Assembly.GetEntryAssembly().GetName().Version}";
            this.Text = lblTitle.Text;
            lblUsername.Text = Setting.Instance.appuser;
            if (!Directory.Exists(Global.LogDir))
                Directory.CreateDirectory(Global.LogDir);
            if (File.Exists($"{Application.StartupPath}\\test.txt"))
                Setting.Instance.isTest = true;
            Setting.Instance.isLicense = isValidLicense();
            //Setting.Instance.browser.start();
            if (!Directory.Exists(Setting.Instance.chromeDir))
                Directory.CreateDirectory(Setting.Instance.chromeDir);
            //Utils.sendTelegram("-4925310309", "!!!Hello!!!", Setting.Instance.typeToken);

            //test();
        }
        private void test()
        {
            double calc = Utils.calcProfit(1.75, 2.354);
            string content = File.ReadAllText($"{Application.StartupPath}\\test.txt");
            //Setting.Instance.spCtrl.getBetResult(content);
            //List<BetOrder> candList = JsonConvert.DeserializeObject<List<BetOrder>>(content);
            //List<BetOrder> list = candList.FindAll(o => o.nOrderStatus == 1).ToList();
            //bool same = Utils.isSameMatch("Los Angeles FC", "Seattle Sndrs", "Los Angeles Galaxy", "Sporting Kansas City");
        }
        private bool isValidLicense()
        {
            try
            {
                bool ret = false;
                HttpClient testClient = new HttpClient();
                HttpResponseMessage msgResponse = testClient.GetAsync("http://38.180.28.251:5558/auth/des6.php").Result;
                msgResponse.EnsureSuccessStatusCode();
                string strResponse = msgResponse.Content.ReadAsStringAsync().Result;
                if (strResponse.Contains("success"))
                    return true;
                return ret;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return false;
            }
        }
        private string getChromePath()
        {
            string ret = string.Empty;
            try
            {
                string cmd = (string)Registry.ClassesRoot.OpenSubKey(@"ChromeHTML\shell\open\command").GetValue(null);
                ret = Regex.Match(cmd, @"""(.*?)""").Groups[1].Value;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
            return ret;
        }
        private void button_Click(object sender, EventArgs e)
        {
            IconButton button = (IconButton)sender;
            panNav.Height = button.Height;
            panNav.Top = button.Top;
            panNav.Left = button.Left;
            resetButtonColor();
            button.BackColor = Color.FromArgb(46, 51, 73);
            hideDlg();
            switch (button.Text)
            {
                case "Main":
                    dlgWork.Show();
                    break;
                case "Singapore":
                    dlgSingapore.Show();
                    break;
                case "Colourhe":
                    dlgSuperodd.Show();
                    break;
                case "Arb":
                    dlgArb.Show();
                    break;
                case "History":
                    dlgHistory.Show();
                    break;
                case "Setting":
                    dlgSetting.Show();
                    break;
                case "Black":
                    dlgBlack.Show();
                    break;
            }
        }
        private void resetButtonColor()
        {
            btnWork.BackColor = Color.FromArgb(24, 30, 54);
            btnSingapore.BackColor = Color.FromArgb(24, 30, 54);
            btnSuperodd.BackColor = Color.FromArgb(24, 30, 54);
            btnArb.BackColor = Color.FromArgb(24, 30, 54);
            btnHistory.BackColor = Color.FromArgb(24, 30, 54);
            btnSetting.BackColor = Color.FromArgb(24, 30, 54);
            btnBlackLeague.BackColor = Color.FromArgb(24, 30, 54);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void frmMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void hideDlg()
        {
            dlgWork.Hide();
            dlgSingapore.Hide();
            dlgSuperodd.Hide();
            dlgArb.Hide();
            dlgHistory.Hide();
            dlgSetting.Hide();
            dlgBlack.Hide();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            stop();
        }

        private void stop()
        {
            //Setting.Instance.spCtrl.stop();
            Setting.Instance.mbCtrl.stop();
            //Setting.Instance.browser.stop();
            //RelationCtrl.Instance.stop();
            Setting.Instance.save();
            dlgWork.stopAutomation();
        }

        private void lblUsername_Click(object sender, EventArgs e)
        {
            if (Setting.Instance.isTest)
            {
                //frmRelation dlg = new frmRelation();
                //dlg.Show();
            }
        }
        private void killProcessByPort()
        {
            try
            {
                List<ProcessPort> psList = ProcessPorts.ProcessPortMap;
                ProcessPort one = ProcessPorts.ProcessPortMap.Find(o => o.PortNumber == Setting.Instance.chromeSocketPort);
                if (one != null)
                    killProcess(one.ProcessId);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"killProcessByPort error - {ex.Message}");
            }
        }
        public void killProcess(int id)
        {
            try
            {
                Process chrome = Process.GetProcessById(id);
                if (chrome != null)
                {
                    chrome.Kill();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"killProcess error - {ex.Message}");
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (Global.bRun)
            {
                if (++tickCount == 60 * 3)      //3분에 한번씩
                    tickCount = 0;
                if (tickCount % 20 == 8)
                {
                    //Setting.Instance.mbCtrl.checkSocket();
                    Global.UpdateSuperodd();
                }
            }
        }
    }
}
