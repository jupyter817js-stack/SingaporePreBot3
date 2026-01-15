using MasterDevs.ChromeDevTools;
using MasterDevs.ChromeDevTools.Protocol.Chrome.DOM;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Network;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Page;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SingaporePreBot3
{
    public class BrowserCtrl
    {
        IChromeProcess chromeProcess = null;
        IChromeSession chromeSession = null;
        int processId;
        long documentNodeId = 1;
        public BrowserCtrl()
        {
            
        }
        public void start()
        {
            Task.Run(() => {
                checkBrowser();
            });
        }
        public void stop()
        {
            try
            {
                Process chrome = Process.GetProcessById(processId);
                if (chrome != null)
                {
                    chrome.Kill();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
        }
        private bool checkBrowser()
        {
            bool ret = false;
            try
            {
                if (chromeProcess != null && chromeSession != null)
                {
                    var sessionInfo = chromeProcess.GetSessionInfo().Result.FirstOrDefault();
                    if (sessionInfo != null)
                        ret = true;
                }
                if (!ret)
                {
                    InitBrowser();
                    var scriptResult = chromeSession.SendAsync(new EvaluateCommand() { Expression = "location.href" }).Result.Result;
                    string url = scriptResult.Result?.Value?.ToString();
                    Trace.WriteLine($"Current Url - {url}");
                    if (url != "https://online.singaporepools.com/en/sports/category/1/football")
                    {
                        scriptResult = chromeSession.SendAsync(new EvaluateCommand() { Expression = "location.href = 'https://online.singaporepools.com/en/sports/category/1/football'" }).Result.Result;
                        Trace.WriteLine($"Script Finished - {JsonConvert.SerializeObject(scriptResult)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"checkBrowser error - {ex.Message}");
            }
            return ret;
        }
        public void InitBrowser()
        {
            try
            {
                ChromeProcessFactory chromeProcessFactory = new ChromeProcessFactory(null);
                chromeProcessFactory.ChromePath = Setting.Instance.chromePath;
                string strPos = $"0,0";
                string strSize = $"1024,768";
                chromeProcess = chromeProcessFactory.Create(Setting.Instance.chromeSocketPort, false, "", Setting.Instance.chromeDir, strPos, strSize);
                processId = ((LocalChromeProcess)chromeProcess).Process.Id;
                Trace.WriteLine($"Process Id - {processId}");
                var sessinList = chromeProcess.GetSessionInfo().Result;
                Trace.WriteLine($"Sessions - {JsonConvert.SerializeObject(sessinList)}");
                var sessionInfo = chromeProcess.GetSessionInfo().Result.ToList().Find(o => o.Type == "page" && (o.Url.StartsWith("chrome://newtab") || o.Url.StartsWith("chrome://welcome")));
                Trace.WriteLine($"Websocket debug url - {sessionInfo.WebSocketDebuggerUrl}");
                var chromeSessionFactory = new ChromeSessionFactory();
                chromeSession = chromeSessionFactory.Create(sessionInfo.WebSocketDebuggerUrl);
                var domEnableResult = chromeSession.SendAsync<MasterDevs.ChromeDevTools.Protocol.Chrome.DOM.EnableCommand>().Result;
                var networkEnableResult = chromeSession.SendAsync<MasterDevs.ChromeDevTools.Protocol.Chrome.Network.EnableCommand>().Result;
                var pageEnableResult = chromeSession.SendAsync<MasterDevs.ChromeDevTools.Protocol.Chrome.Page.EnableCommand>().Result;
                var runTimeEnableResult = chromeSession.SendAsync<MasterDevs.ChromeDevTools.Protocol.Chrome.Runtime.EnableCommand>().Result;
                var navigateResponse = chromeSession.SendAsync(new NavigateCommand
                    {
                        Url = $"https://online.singaporepools.com/en/sports/category/1/football"
                    }
                ).Result;
                //chromeSession.Subscribe<ResponseReceivedEvent>(e =>
                //{
                //    Task.Run(async () =>
                //    {
                //        //try
                //        //{
                //        //    if (e.Response.Status != 200)
                //        //        return;
                //        //    var url = e.Response.Url;
                //        //    if (url.Contains("categoryName=football&mode=web"))
                //        //    {
                //        //        //var result = (await chromeSession.SendAsync(new GetResponseBodyCommand() { RequestId = e.RequestId })).Result;
                //        //        //string RespBody = result.Body;
                //        //        //Trace.WriteLine($"{url} - {RespBody}");
                //        //    }
                //        //}
                //        //catch (Exception ex)
                //        //{
                //        //    Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
                //        //}
                //    });
                //});

                //chromeSession.Subscribe<WebSocketFrameReceivedEvent>(e =>
                //{
                //    string strData = e.Response.PayloadData;
                //    if (!string.IsNullOrEmpty(strData))
                //    {
                //        parseSocketData(strData, 1);
                //    }
                //});
                chromeSession.Subscribe<RequestWillBeSentEvent>(req =>
                {
                    // we cannot block in event handler, hence the task
                    Task.Run(async () =>
                    {
                        try
                        {
                            var url = req.Request.Url;
                            if (url.Contains("mfp/api/adapters/spplMfpApi"))
                            {
                                //Trace.WriteLine("Prematch page coming");
                                Dictionary<string, string> header = req.Request.Headers;
                                if (header == null)
                                    return;
                                if (header.ContainsKey("Authorization"))
                                    Setting.Instance.spCtrl.authToken = header["Authorization"];
                                if (header.ContainsKey("x-mfp-analytics-metadata"))
                                    Setting.Instance.spCtrl.metaData = header["x-mfp-analytics-metadata"];
                                if (header.ContainsKey("x-session-id"))
                                    Setting.Instance.spCtrl.sessionId = header["x-session-id"];
                                Setting.Instance.spCtrl.initHttpClient();
                            }
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
                        }
                        await Task.CompletedTask; // Added to ensure the method is properly awaited
                    });
                });

                chromeSession.Subscribe<LoadEventFiredEvent>(loadEventFired =>
                {
                    // we cannot block in event handler, hence the task
                    Task.Run(async () =>
                    {
                        documentNodeId = (await chromeSession.SendAsync(new GetDocumentCommand())).Result.Root.NodeId;
                    });
                });
                if (documentNodeId == 1)
                {
                }
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
                string script = "var authToken = null;WL.AuthorizationManager.obtainAccessToken('RegisteredClient accessRestricted').then(tt =>{authToken = tt.asAuthorizationRequestHeader;});";  //(await WL.AuthorizationManager.obtainAccessToken('RegisteredClient accessRestricted')).asAuthorizationRequestHeader;
                //string script = "var authToken = null; authToken = JSON.parse(sessionStorage['sg.com.sgpools.app.com.mfp.scope.token.mapping'])['RegisteredClient'].value;";
                var scriptResult = chromeSession.SendAsync(new EvaluateCommand() { Expression = script }).Result.Result;
                Thread.Sleep(50);
                scriptResult = chromeSession.SendAsync(new EvaluateCommand() { Expression = "authToken" }).Result.Result;
                //token = "Bearer " + scriptResult.Result?.Value?.ToString();
                token = scriptResult.Result?.Value?.ToString();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
            return token;
        }
        public void getCookie()
        {
            try
            {
                GetAllCookiesCommandResponse cookieResult = chromeSession.SendAsync(new MasterDevs.ChromeDevTools.Protocol.Chrome.Network.GetAllCookiesCommand()).Result.Result;
                Setting.Instance.spContainer = new CookieContainer();
                if (cookieResult == null || cookieResult.Cookies.Length > 0)
                {
                    foreach (MasterDevs.ChromeDevTools.Protocol.Chrome.Network.Cookie one in cookieResult.Cookies)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(one.Domain))
                                Setting.Instance.spContainer.Add(new System.Net.Cookie(one.Name, one.Value, one.Path, one.Domain));
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine($"add cookie error - {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
        }
        public void refreshBrowser()
        {
            try
            {
                var results = chromeSession.SendAsync(new ReloadCommand()).Result;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
        }
        public void keepLogin()
        {
            try
            {
                executeScript("if (document.getElementById('modal-session-about-to-expire') !== null) {document.getElementById('modal-session-about-to-expire-extend-button').click();}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
        }
        public void gotoUrl(string url)
        {
            try
            {
                var navigateResponse = chromeSession.SendAsync(new NavigateCommand
                {
                    Url = url
                }).Result;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
        }
        public string executeScript(string script)
        {
            string ret = string.Empty;
            try
            {
                var scriptResult = chromeSession.SendAsync(new EvaluateCommand() { Expression = script }).Result.Result;
                ret = scriptResult.Result?.Value?.ToString();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
            return ret;
        }
    }
}
