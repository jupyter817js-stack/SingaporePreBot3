using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SingaporePreBot3
{
    public partial class frmWork : Form
    {
        public frmWork()
        {
            InitializeComponent();
        }

        private void frmWork_Load(object sender, EventArgs e)
        {
            //test();
        }

        private void test()
        {
            //bool ret = Utils.isSameMatch("Hatta Dubai", "Al Dhaid", "Fortuna Sittard", "Alkmaar");
            //byte[] content = File.ReadAllBytes($"{Application.StartupPath}\\captcha.png");
            //string ret = Utils.trySolvingCaptchaByImg(content);
            string content = File.ReadAllText($"{Application.StartupPath}\\test.txt");
            //string bettingId = Regex.Match(content, @"\|r=(?<VAL>[^|]*)").Groups["VAL"].Value;
            //double odd = Utils.ParseToDouble(Regex.Match(content, "@ (?<VAL>[^\n]*)").Groups["VAL"].Value);
            List<MatchItem> newMatches = Setting.Instance.spCtrl.parseEventContent(content);
        }
        public void writeStatus(string status)
        {
            try
            {
                string content = $"[{DateTime.Now.ToString("HH:mm:ss")}] {status}";
                string logPath = $"{Global.LogDir}\\{DateTime.Now.ToString("yyyyMMdd")}.txt";
                txtLog.BeginInvoke(new Action(() =>
                {
                    string prefix = string.IsNullOrEmpty(txtLog.Text) ? "" : Environment.NewLine;
                    string logText = $"{prefix}{content}";
                    addLog(logText);
                    txtLog.ScrollToCaret();
                }));
                using (FileStream fs = File.Open(logPath, FileMode.Append, FileAccess.Write))
                {
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        writer.WriteLine(content);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
        }

        private void addLog(string logText)
        {
            try
            {
                int num = txtLog.TextLength;
                txtLog.AppendText(logText);
                txtLog.Select(num, logText.Length);
                txtLog.SelectionColor = logText.Contains("미적중") ? Color.FromArgb(0, 146, 249) : (logText.Contains("적중") ? Color.Red : (logText.Contains("타이") ? Color.FromArgb(50, 226, 178) : Color.White));
                txtLog.Select(txtLog.TextLength, 0);
            }
            catch { }

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                if (!canStart())
                    return;
                Setting.Instance.save();
                if (btnStart.Text == "Start")
                {
                    btnStart.IconChar = FontAwesome.Sharp.IconChar.Stop;
                    btnStart.Text = "Stop";
                    startWork();
                }
                else
                {
                    stopAutomation(true);
                    btnStart.IconChar = FontAwesome.Sharp.IconChar.Play;
                    btnStart.Text = "Start";
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"btnStart_Click error - {ex.Message}");
            }
        }

        private bool canStart()
        {
            if (string.IsNullOrEmpty(Setting.Instance.domainSingapore) || string.IsNullOrEmpty(Setting.Instance.usernameSingapore) || string.IsNullOrEmpty(Setting.Instance.passwordSingapore) || string.IsNullOrEmpty(Setting.Instance.domainSuperodd) || string.IsNullOrEmpty(Setting.Instance.usernameSuperodd) || string.IsNullOrEmpty(Setting.Instance.passwordSuperodd))
            {
                new frmMsg("Please input account information correctly").ShowDialog();
                return false;
            }
            if (Setting.Instance.stakeList.Count == 0)
            {
                new frmMsg("Please input stake information correctly").ShowDialog();
                return false;
            }
            return true;
        }

        private void startWork()
        {
            Global.bRun = true;
            BettingController.Instance.startAutomation();
        }

        public void stopAutomation(bool force = false)
        {
            try
            {
                Global.bRun = false;
                btnStart.BeginInvoke((Action)delegate ()
                {
                    btnStart.Text = "Start";
                    btnStart.IconChar = FontAwesome.Sharp.IconChar.Play;
                });
                BettingController.Instance.stopAutomation();
            }
            catch
            {
            }
        }

        public void updateStatus()
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    lblBalanceDafabet.Text = Setting.Instance.balanceSingapore.ToString("N2");
                    lblBalancePinnacle.Text = Setting.Instance.balanceSuperodd.ToString("N2");
                }));
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {
            if (Setting.Instance.isTest)
            {
                frmTest dlg = new frmTest();
                dlg.Show();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Task.Run(() => {
                Setting.Instance.spCtrl.getBalance();
            });
        }

        private void label3_Click(object sender, EventArgs e)
        {
            //if (Setting.Instance.isTest)
            //{
            //    Task.Run(() => {
            //        Setting.Instance.chCtrl.getBalance();
            //    });
            //}
            Task.Run(() => {
                string token = Global.GetAuthToken();
            });
        }
    }
}
