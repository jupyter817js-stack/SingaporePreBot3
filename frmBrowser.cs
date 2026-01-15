using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace RichBot
{
    public partial class frmBrowser : Form
    {
        public frmBrowser()
        {
            InitializeComponent();
        }

        private void frmBrowser_Load(object sender, EventArgs e)
        {
            initWebView();

        }
        #region Webview2 Function
        private void initWebView()
        {
            try
            {
                browser.Source = new Uri($"https://{Setting.Instance.domainSingapore}/en/sports");
                //CoreWebView2EnvironmentOptions Options = new CoreWebView2EnvironmentOptions();
                //CoreWebView2Environment env = CoreWebView2Environment.CreateAsync(null, null, Options).Result;
                //browser.EnsureCoreWebView2Async(env);
                browser.CoreWebView2InitializationCompleted += initializationCompleted;
                browser.NavigationCompleted += navigateFinished;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
        }
        private void initializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            try
            {
                if (!e.IsSuccess)
                {
                    Trace.WriteLine($"WebView2 creation failed with exception = {e.InitializationException}");
                    return;
                }
                browser.CoreWebView2.Settings.IsPasswordAutosaveEnabled = true;
                browser.CoreWebView2.WebResourceResponseReceived += responseReceived;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
        }
        private void navigateFinished(object sender, CoreWebView2NavigationCompletedEventArgs arg)
        {
            Trace.WriteLine("Navigation finished");
        }
        private string openUrl(string url)
        {
            string ret = "";
            this.BeginInvoke(new Action(() =>
            {
                Trace.WriteLine($"Open Url - {url}");
                browser.CoreWebView2.Navigate(url);
                Trace.WriteLine("Navigation requested");
            }));
            return ret;
        }
        public void removeCookies()
        {
            this.Invoke(new Action(() =>
            {
                browser.CoreWebView2.CookieManager.DeleteAllCookies();
            }));
        }
        private string runScript(string code)
        {
            string ret = string.Empty;
            bool isFinish = false;
            try
            {
                browser.BeginInvoke(new Action(async () =>
                {
                    ret = await browser.CoreWebView2.ExecuteScriptAsync(code);
                    isFinish = true;
                }));
                while (!isFinish)
                    Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
            return ret;
        }
        public void getCookie()
        {
            bool isFinish = false;
            browser.BeginInvoke(new Action(async () =>
            {
                List<CoreWebView2Cookie> cookies = await browser.CoreWebView2.CookieManager.GetCookiesAsync($"https://{Setting.Instance.domainSingapore}/");
                Setting.Instance.spContainer = new CookieContainer();
                foreach (CoreWebView2Cookie x in cookies)
                    Setting.Instance.spContainer.Add(new Cookie(x.Name, x.Value, x.Path, x.Domain));
                isFinish = true;
            }));
            while (!isFinish)
                Thread.Sleep(100);
        }
        public void refreshBrowser()
        {
            try
            {
                this.BeginInvoke(new Action(() =>
                {
                    browser.CoreWebView2.Reload();
                }));
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
        }
        public string getAuthToken()
        {
            string token = string.Empty;
            try
            {
                token = runScript("WL.AuthorizationManager.obtainAccessToken('RegisteredClient accessRestricted').then(ret => { console.log(ret)})");  //(await WL.AuthorizationManager.obtainAccessToken('RegisteredClient accessRestricted')).asAuthorizationRequestHeader;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
            return token;
        }
        private async void responseReceived(object sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
        {
            try
            {
                int status = e.Response.StatusCode;
                if (status == 200)
                {
                    try
                    {
                        if (e.Request.Uri.Contains("categoryName=football&mode=web"))
                        {
                            CoreWebView2HttpRequestHeaders header = e.Request.Headers;
                            if (header.Contains("Authorization"))
                                Setting.Instance.spCtrl.authToken = header.GetHeader("Authorization");
                            if (header.Contains("x-mfp-analytics-metadata"))
                                Setting.Instance.spCtrl.metaData = header.GetHeader("x-mfp-analytics-metadata");
                            if (header.Contains("x-session-id"))
                                Setting.Instance.spCtrl.sessionId = header.GetHeader("x-session-id");
                            Setting.Instance.spCtrl.initHttpClient();
                            System.IO.Stream content = await e.Response.GetContentAsync();
                            //if (content == null)
                            //    return;
                            //StreamReader reader = new StreamReader(content);
                            //string strData = reader.ReadToEnd();
                        }
                    }
                    catch (Exception ex)
                    {
                        // A COMException will be thrown if the content failed to load.
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
        }
        #endregion
    }
}
