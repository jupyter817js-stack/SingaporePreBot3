using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace SingaporePreBot3
{
    public partial class frmLogin : Form
    {
        Thread th;
        string loginUrl = "http://185.181.9.205/api/CheckLogin";
        public frmLogin()
        {
            InitializeComponent();
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            Trace.WriteLine($"File version - {Assembly.GetEntryAssembly().GetName().Version}");
            txtUsername.Text = Setting.Instance.ReadRegistry("appuser");
            txtPassword.Text = Setting.Instance.ReadRegistry("apppass");
        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername.Text))
            {
                frmMsg msgYesDialog = new frmMsg("Please input username");
                msgYesDialog.ShowDialog();
                return;
            }
            if (string.IsNullOrEmpty(txtPassword.Text))
            {
                frmMsg msgYesDialog = new frmMsg("Please input password");
                msgYesDialog.ShowDialog();
                return;
            }
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage html;
            string str = $"&username={txtUsername.Text}&password={txtPassword.Text}";
            string gameParam = Utils.encrypt(str);
            try
            {
                html = httpClient.PostAsync(loginUrl, new StringContent(gameParam, Encoding.UTF8, "application/json")).Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Network Error ! ", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string result = html.Content.ReadAsStringAsync().Result;
            result = Utils.decrypt(result.Replace("\"", ""));
            string msg = Regex.Match(result, @"msg"":""(?<msg>[^""]*)").Groups["msg"].Value;
            if (msg == "success")
            {
                string limittime = Regex.Match(result, @"Time"":""(?<Time>[^\""]*)").Groups["Time"].Value;
                DateTime endDate = DateTime.Parse(limittime);
                if (DateTime.Now > endDate)
                {
                    MessageBox.Show("Account expired!", "Alert");
                    return;
                }
            }
            else
            {
                string errMsg = msg;
                if (msg == "Login Fail")
                    errMsg = "Account and Password is not correct !";
                else if (msg == "stopped")
                    errMsg = "Account is blocked!";
                else if (msg.Contains("other side"))
                    errMsg = "License is working in other side !";
                MessageBox.Show(errMsg, "Alert");
                return;
            }
            Setting.Instance.appuser = txtUsername.Text;
            Setting.Instance.WriteRegistry("appuser", Setting.Instance.appuser);
            Setting.Instance.WriteRegistry("apppass", txtPassword.Text);
            try
            {
                th = new Thread(openNewForm);
                th.SetApartmentState(ApartmentState.STA);
                th.Start();
                this.Close();
            }
            catch { }
        }

        private void openNewForm(object obj)
        {
            try
            {
                Application.Run(new frmMain());
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} - error {ex.Message}");
            }
        }

        private void lblExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                btnLogin_Click(sender, e);
            }
        }

        private void frmLogin_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                btnLogin_Click(sender, e);
            }
        }
        public void checkingVersion()
        {
            bool flag = false;
            int Cnt = 0;
            do
            {
                try
                {
                    if (!File.Exists(@"updater.exe"))
                    {
                        Application.ExitThread();
                        Environment.Exit(0);
                        return;
                    }

                    HttpClient httpClient = new HttpClient();
                    var html = httpClient.GetAsync(loginUrl + "/api/settings/version").Result;
                    html.EnsureSuccessStatusCode();
                    string strhtml = html.Content.ReadAsStringAsync().Result;

                    string pattern = @"version"":""(?<VAL>[^\""]*)";
                    Match m = Regex.Match(strhtml, pattern);

                    string version = m.Groups["VAL"].Value;
                    if (version != "Fail" && version != Assembly.GetEntryAssembly().GetName().Version.ToString())
                    {
                        Process.Start(@"updater.exe");
                        this.Close();
                        Application.Exit();
                    }
                    flag = true;
                    break;
                }
                catch
                {
                    Cnt++;
                }
            }
            while (Cnt != 3);

            if (!flag)
            {
                MessageBox.Show("서비스오류 ! ", "알림");
                Application.ExitThread();
                Environment.Exit(0);
            }
        }
    }
}
