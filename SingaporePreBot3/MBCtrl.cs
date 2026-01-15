using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Shapes;
using WebSocketSharp;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace SingaporePreBot3
{
    public class MBCtrl : ISiteController
    {
        private CookieContainer cookieContainer = null;
        private HttpClient httpClient = null;
        public bool isWorking = false;
        Thread mainThread = null;
        Thread marketThread = null;
        Thread balanceThread = null;
        int loginStatus;
        long counter = Utils.getTick();
        Random rnd = new Random();
        DateTime balanceTime = DateTime.Now;
        DateTime checkTime = DateTime.Now;
        bool isLogin = false;
        string session_id;
        string username;
        long tick = 0;
        private string lid = "";
        private string strPreMatchUrl = "";

        WebSocket webSocket = null;
        int socketStatus = 0;
        List<MatchItem> matches = new List<MatchItem>();

        int limitHours = 24;

        private string strViewState;
        private string strViewStateGenerator;
        private string strViewEnentValidation;

        public MBCtrl()
        {
        }

        public void start()
        {
            try
            {
                if (mainThread == null || !mainThread.IsAlive)
                {
                    isWorking = true;
                    Global.WriteStatus("[Superodd] Program Started.");
                    mainThread = new Thread(() => run());
                    mainThread.Start();
                    marketThread = new Thread(() => getOldNewMarkets());
                    marketThread.Start();
                    balanceThread = new Thread(checkLogin);
                    balanceThread.Start();
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }
        private void run()
        {
            try
            {
                initHttpClient();
                login();
                //getBalance();
                //doConnect();
                while (true)
                {
                    //if (Setting.Instance.isLicense)
                    {
                        getPrematchEvents();
                    }
                    Thread.Sleep(10000 * Setting.Instance.superoddTime);
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }
    
        public void getPrematchEvents()
        {
            string oddTodayUrl = $"https://{Setting.Instance.domainSuperodd}/_View/RMOdds1.aspx?ot=t";
            string oddEarlyUrl = $"https://{Setting.Instance.domainSuperodd}/_View/RMOdds1.aspx?ot=e";
            
            string typeUrl = $"https://{Setting.Instance.domainSuperodd}/_View/RMOdds2.aspx?ot=t&ov=0&mt=0&wd=&isWC=0&ia=0&tf=-1&accType=EU";

            try
            {
                HttpResponseMessage oddMessage = httpClient.GetAsync(oddTodayUrl).Result;
                oddMessage.EnsureSuccessStatusCode();
                string strOddContent = oddMessage.Content.ReadAsStringAsync().Result;

                //Set to EU Mode
                HttpResponseMessage typeMessage = httpClient.GetAsync(typeUrl).Result;
                typeMessage.EnsureSuccessStatusCode();

                //string strRunPattern = @"ajaxRun[^']*'(?<val>[^']*)";
                string strTodayPattern = @"ajaxToday[^']*'(?<val>[^']*)";

                //string strLiveUrl = $"https://{Setting.Instance.domainSuperodd}/_View/" + Regex.Match(strOddContent, strRunPattern).Groups["val"].Value;
                strPreMatchUrl = $"https://{Setting.Instance.domainSuperodd}/_View/" + Regex.Match(strOddContent, strTodayPattern).Groups["val"].Value;

                if (tick == 0)
                    tick = Utils.getTick();
                else
                    tick++;

                strPreMatchUrl = strPreMatchUrl + $"&LID={lid}&_={tick}";
                HttpResponseMessage preMatchMessage = httpClient.GetAsync(strPreMatchUrl).Result;
                string strLiveContent = preMatchMessage.Content.ReadAsStringAsync().Result;

                List<MatchItem> matchItems = GetMatchItems(strLiveContent);
                if (RelationCtrl.Instance.getMatches("MB").Count == 0)
                {
                    RelationCtrl.Instance.setMatches("MB", matchItems);
                }
                else
                {
                    RelationCtrl.Instance.UpdateMatches("MB", matchItems);
                }

                Global.UpdateSuperodd();
                //RelationCtrl.Instance.checkSiteCandidate("MB");
            }
            catch (Exception e)
            {
                Trace.WriteLine("Try GetLiveEvents Invalid!");
            }
        }

        private void getOldNewMarkets()
        {
            int startTime = Setting.Instance.startTime;
            int endTime = Setting.Instance.endTime;
            int deltaTime = 5;
            try
            {
                while (isWorking)
                {
                    foreach (var match in RelationCtrl.Instance.getMatches("MB"))
                    {
                        var now = DateTime.UtcNow.AddHours(8);

                        // 예시: startTime - 150분 전 oldMarkets 채우기
                        if (now >= match.startTime.AddMinutes(-startTime) && now <= match.startTime.AddMinutes(-startTime + deltaTime) && match.oldMarkets.Count == 0)
                        {
                            match.oldMarkets = FetchMarkets(match.matchId);
                        }

                        // 예시: startTime - 30분 전 newMarkets 채우기
                        if (now >= match.startTime.AddMinutes(-endTime) && now <= match.startTime.AddMinutes(-endTime + deltaTime) && match.newMarkets.Count == 0)
                        {
                            match.newMarkets = FetchMarkets(match.matchId);
                        }
                    }
                    RelationCtrl.Instance.doCompare("MB");
                    if ((Setting.Instance.compareResult.Count > 0))
                    {
                        Utils.sendTelegram("-4925310309", string.Join(Environment.NewLine, Setting.Instance.compareResult), Setting.Instance.typeToken);
                        Global.WriteStatus(string.Join(Environment.NewLine, Setting.Instance.compareResult));
                    }

                    Global.UpdateSuperodd();
                    Thread.Sleep(10000); // 10초 주기
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"getOldNewMarkets error - {ex.Message}");
            }
        }

        private List<Market> FetchMarkets(string matchId)
        {
            // 서버 요청 → Market 리스트 변환
            HttpResponseMessage preMatchMessage = httpClient.GetAsync(strPreMatchUrl).Result;
            string strLiveContent = preMatchMessage.Content.ReadAsStringAsync().Result;
            List<MatchItem> matchItems = GetMatchItems(strLiveContent);
            MatchItem match = matchItems.Find(m => m.matchId == matchId);
            List<Market> marketList = match.markets;
            return marketList;
        }
        public MatchItem GetMatchItem(dynamic game, string leagueId, string leagueName, bool isOne)
        {
            MatchItem matchItem = null;
            try
            {
                matchItem = new MatchItem();
                matchItem.isOne = isOne;
                matchItem.leagueId = leagueId;
                matchItem.leagueName = leagueName;
                //matchItem.matchId = game[86]?.ToString() == "0" ? game[64]?.ToString() : game[86]?.ToString();
                matchItem.home = game[9]?.ToString() ?? string.Empty;
                string str = game[8]?.ToString().Contains("/") ? game[8]?.ToString() : DateTime.Now.ToString("dd") + "/" + DateTime.Now.ToString("MM") + " " + game[8]?.ToString();
                matchItem.startTime = Utils.ParseToDateTime(str, "dd/MM hh:mmtt");
                matchItem.away = game[10]?.ToString() ?? string.Empty;
                int period = Utils.ParseToInt(game[60]?.ToString() ?? string.Empty);
                int minute = Utils.ParseToInt(game[59]?.ToString() ?? string.Empty);
                matchItem.minute = period == 2 ? 45 + minute : minute;
                matchItem.homeScore = Utils.ParseToInt(game[38]?.ToString() ?? string.Empty);
                matchItem.awayScore = Utils.ParseToInt(game[39]?.ToString() ?? string.Empty);
                string fullTimeId = game[0]?.ToString() ?? string.Empty;
                string halfTimeId = game[1]?.ToString() ?? string.Empty;
                matchItem.matchId = matchItem.home + "&" + matchItem.away;

                if (fullTimeId != "0")
                {
                    string hdInitLine = game[13]?.ToString() == "-1" ? "" : game[13]?.ToString() ?? "";
                    bool hdFlag = game[11] == "1" ? true : false;
                    if (!string.IsNullOrEmpty(hdInitLine))
                    {
                        string strLine = hdFlag == false ? $"{hdInitLine}" : $"-{hdInitLine}";
                        double ahHomeOdd = Utils.ParseToDouble(game[88]?.ToString() ?? "") / 10;
                        double ahAwayOdd = Utils.ParseToDouble(game[89]?.ToString() ?? "") / 10;

                        double overOdd = Utils.ParseToDouble(game[92]?.ToString() ?? "") / 10;
                        double underOdd = Utils.ParseToDouble(game[93]?.ToString() ?? "") / 10;
                        string ouLine = game[19]?.ToString() == "-1" ? "" : game[19]?.ToString() ?? "";

                        double homeOdd = Utils.ParseToDouble(game[67]?.ToString() ?? "");
                        double awayOdd = Utils.ParseToDouble(game[69]?.ToString() ?? "");
                        double drawOdd = Utils.ParseToDouble(game[68]?.ToString() ?? "");

                        //AH Add
                        if ((ahHomeOdd != 1 || ahAwayOdd != 1))
                            matchItem.markets.Add(new Market(0, 0, matchItem.matchId, fullTimeId, strLine, ahHomeOdd, ahAwayOdd, "", "", ""));

                        //AH Add
                        if ((homeOdd != 1 || awayOdd != 1) && isOne)
                            matchItem.markets.Add(new Market(2, 0, matchItem.matchId, fullTimeId, drawOdd.ToString(), homeOdd, awayOdd, "", "", ""));

                        //OU Add
                        //if (!((overOdd == 1) && (underOdd == 1)))
                        //    matchItem.markets.Add(new Market(1, 0, matchItem.matchId, fullTimeId, ouLine, overOdd, underOdd, "", "", ""));
                    }
                }
                //if (halfTimeId != "0")
                //{
                //    string hdInitLine = game[16]?.ToString() == "-1" ? "" : game[16]?.ToString() ?? "";

                //    bool hdFlag = game[12] == "1" ? true : false;
                //    if (!string.IsNullOrEmpty(hdInitLine))
                //    {
                //        string strLine = hdFlag == false ? $"{hdInitLine}" : $"-{hdInitLine}";
                //        double ahHomeOdd = Utils.ParseToDouble(game[90]?.ToString() ?? "") / 10;
                //        double ahAwayOdd = Utils.ParseToDouble(game[91]?.ToString() ?? "") / 10;

                //        double overOdd = Utils.ParseToDouble(game[94]?.ToString() ?? "") / 10;
                //        double underOdd = Utils.ParseToDouble(game[95]?.ToString() ?? "") / 10;
                //        string ouLine = game[22]?.ToString() == "-1" ? "" : game[22]?.ToString() ?? "";

                //        double homeOdd = Utils.ParseToDouble(game[70]?.ToString() ?? "") / 10;
                //        double awayOdd = Utils.ParseToDouble(game[72]?.ToString() ?? "") / 10;
                //        double drawOdd = Utils.ParseToDouble(game[71]?.ToString() ?? "") / 10;

                //        //AH Add
                //        if (!((ahHomeOdd == 1) && (ahHomeOdd == 1)))
                //            matchItem.markets.Add(new Market(0, 1, matchItem.matchId, halfTimeId, strLine, ahHomeOdd, ahAwayOdd, "", "", ""));

                //        //OU Add
                //        if (!((overOdd == 1) && (underOdd == 1)))
                //            matchItem.markets.Add(new Market(1, 1, matchItem.matchId, halfTimeId, ouLine, overOdd, underOdd, "", "", ""));
                //    }
                //}
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Getting Match Item Invalid- {e.Message}");
            }
            return matchItem;
        }
        public static bool isOneMarket(string matchId, dynamic league)
        {
            bool flag = true;
            int cnt = 0;
            foreach (JArray game in league)
            {
                if ((game[9]?.ToString() ?? string.Empty) + "&" + (game[10]?.ToString() ?? string.Empty) == matchId)
                {
                    cnt++;
                }
            }
            if (cnt > 1) flag = false;
            else flag = true;
            return flag;
        }
        public List<MatchItem> GetMatchItems(string matchContents)
        {
            List<MatchItem> matchItems = new List<MatchItem>();
            if (string.IsNullOrEmpty(matchContents))
                return matchItems;
            try
            {
                JArray array = JArray.Parse(matchContents);
                if (array == null || array.Count < 3)
                    return matchItems;
                string first = array[0][0]?.ToString() ?? string.Empty;
                //lid = array[0][1]?.ToString() ?? String.Empty;
                JArray leagues = first == "1" ? (JArray)array[2] : (JArray)array[3];

                foreach (JArray eachLeague in leagues)
                {
                    if (eachLeague.Count < 2)
                        continue;
                    string leagueId = eachLeague[0][0]?.ToString() ?? string.Empty;
                    string leagueName = eachLeague[0][1]?.ToString() ?? string.Empty;

                    if (leagueName.Contains("SABA ") ||
                        leagueName.Contains("- CORNERS") ||
                        leagueName.Contains(" TOTAL GOAL") ||
                        leagueName.Contains("- BOOKING") ||
                        leagueName.Contains("- OFFSIDE") ||
                        leagueName.Contains("- WINNER") ||
                        leagueName.Contains("WHICH TEAM WILL ADVANCE TO NEXT ROUND") ||
                        leagueName.Contains("FANTASY MATCH") ||
                        leagueName.Contains("NO BET") ||
                        leagueName.Contains("TEST") ||
                        leagueName.Contains("OVER/UNDER") ||
                        leagueName.Contains("Bitcoin"))
                        continue;
                    if (Setting.Instance.blackLeagues.Contains(leagueName))
                        continue;
                    foreach (JArray eachGame in eachLeague[1])
                    {
                        MatchItem newItem = null;
                        if (isOneMarket((eachGame[9]?.ToString() ?? string.Empty) + "&" + (eachGame[10]?.ToString() ?? string.Empty), eachLeague[1]))
                        {
                            //Add under/over market
                            newItem = GetMatchItem(eachGame, leagueId, leagueName, true);

                        }
                        else
                        {
                            newItem = GetMatchItem(eachGame, leagueId, leagueName, false);

                        }
                        if (newItem != null)
                        {
                            MatchItem currentItem = newItem.matchId == "0" ? null : matchItems.Find(cur => cur.matchId == newItem.matchId);
                            if (currentItem != null)
                            {
                                currentItem.markets.AddRange(newItem.markets);
                                //currentItem.UpdateMarkets(newItem.markets);
                            }
                            else
                            {
                                DateTime nowTime = DateTime.UtcNow.AddHours(8);
                                if (newItem.startTime < nowTime)
                                {
                                    newItem.startTime = newItem.startTime.AddHours(24);
                                }
                                matchItems.Add(newItem);
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {

            }
            finally
            {
                List<int> oldList = new List<int>();
                for (int i = 0; i < matchItems.Count; i++)
                {
                    DateTime nowTime = DateTime.UtcNow.AddHours(8);
                    if (matchItems[i].startTime < nowTime)
                        oldList.Add(i);
                    else
                        continue;
                }
                foreach (int index in oldList.OrderByDescending(i => i))
                {
                    matchItems.RemoveAt(index);
                }
            }
            return matchItems;
        }
        public void initHttpClient()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            cookieContainer = new CookieContainer();
            handler.CookieContainer = cookieContainer;
            httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Setting.Instance.userAgent);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9,ko;q=0.8");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }

        public void InitLoginFormDatas()
        {
            try
            {
                HttpResponseMessage httpResponse = httpClient.GetAsync($"https://{Setting.Instance.domainSuperodd}/Default.aspx?lang=EN-US").Result;
                httpResponse.EnsureSuccessStatusCode();
                string strInitPage = httpResponse.Content.ReadAsStringAsync().Result;

                string strViewStatePattern = "id=\"__VIEWSTATE\" value=\"(?<val>[^\"]*)";
                string strViewStateGenPattern = "id=\"__VIEWSTATEGENERATOR\" value=\"(?<val>[^\"]*)";
                string strEventValPattern = "id=\"__EVENTVALIDATION\" value=\"(?<val>[^\"]*)";

                strViewState = Regex.Match(strInitPage, strViewStatePattern).Groups["val"].Value;
                strViewStateGenerator = Regex.Match(strInitPage, strViewStateGenPattern).Groups["val"].Value;
                strViewEnentValidation = Regex.Match(strInitPage, strEventValPattern).Groups["val"].Value;
            }
            catch (Exception ex)
            {
                Global.WriteStatus("InitLoginFormDatas Invalid: -" + ex.Message.ToString());
            }
        }
        private void checkLogin()
        {
            while (isWorking)
            {
                try
                {
                    Thread.Sleep(1000);
                    if (checkTime.AddMinutes(1) < DateTime.Now)
                    {
                        //httpClient.DefaultRequestHeaders.Remove("session");
                        //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("session", session_id);
                        ////httpClient.DefaultRequestHeaders.Remove("authorization");
                        ////httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", session_id);
                        //httpClient.DefaultRequestHeaders.Referrer = new Uri($"https://{Setting.Instance.domainSuperodd}/trade");
                        //HttpResponseMessage loginMessage = httpClient.GetAsync($"https://{Setting.Instance.domainSuperodd}/web/sessions/{session_id}/ticker/").Result;
                        //string content = loginMessage.Content.ReadAsStringAsync().Result;
                        //JObject obj = JObject.Parse(content);
                        //string status = obj["status"]?.ToString();
                        isLogin = false;
                        Uri uri = new Uri($"https://{Setting.Instance.domainSuperodd}/Defaults/Default1.aspx");
                        var cookies = cookieContainer.GetCookies(uri);
                        foreach (Cookie cookie in cookies)
                        {
                            if (cookie.Name == ".ASPXAUTH")
                                isLogin = true;
                        }

                        if (!isLogin)
                        {
                            Global.WriteStatus($"[Superodd] Logout");
                            login();
                        }
                        checkTime = DateTime.Now;
                    }
                    //if (balanceTime.AddMinutes(5) < DateTime.Now)
                    //    getBalance();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
                }
            }
            
        }
        public void getLiveEvents()
        {
            try
            {
                //if (tick == 0)
                //    tick = Utils.getTick();
                //else
                //    tick++;
                //string url = $"https://{Setting.Instance.domainSuperodd}/_View/svPage/RMOdds1Gen.ashx?ov=0&ot=r&tf=-1&TFStatus=0&update=false&r={r}&mt=0&wd=&ia=0&isWC=0&isSiteFav=False&LID=&_={tick}";            //{lid}
                //HttpResponseMessage liveMessage = httpClient.GetAsync(url).Result;
                //string liveContent = liveMessage.Content.ReadAsStringAsync().Result;

                //if (string.IsNullOrEmpty(liveContent))
                //    return;
                //updateTime = DateTime.Now;
                //List<MatchItem> matches = parseEventContent(liveContent);
                //if (matches != null)
                //{
                //    foreach (MatchItem one in matches)
                //        RelationCtrl.Instance.updateMatch("MB", one);
                //    Global.UpdateSuperodd();
                //}
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
        }
        public List<Market> parseMarketContent(JObject obj, string matchId, int period)
        {
            List<Market> ret = new List<Market>();
            if (obj == null)
                return ret;
            try
            {
                if (obj["ah"] != null && obj["ah"].Count() > 1 && obj["ah"][1].Count() > 1)
                {
                    double homePrice = Utils.ParseToDouble(obj["ah"][1][1][1]?.ToString() ?? "");
                    double awayPrice = Utils.ParseToDouble(obj["ah"][1][0][1]?.ToString() ?? "");
                    double ahLine = Utils.ParseToDouble(obj["ah"][0]?.ToString() ?? "") / 4;
                    if (homePrice > 1 && awayPrice > 1)
                        ret.Add(new Market(1, period, matchId, "", $"{ahLine}", homePrice, awayPrice, "", "", ""));
                }
                if (obj["ahou"] != null && obj["ahou"].Count() > 1 && obj["ahou"][1].Count() > 1)
                {
                    double overPrice = Utils.ParseToDouble(obj["ahou"][1][0][1]?.ToString() ?? "");
                    double underPrice = Utils.ParseToDouble(obj["ahou"][1][1][1]?.ToString() ?? "");
                    double ouLine = Utils.ParseToDouble(obj["ahou"][0]?.ToString() ?? "") / 4;
                    if (overPrice > 1 && underPrice > 1)
                        ret.Add(new Market(2, period, matchId, "", $"{ouLine}", overPrice, underPrice, "", "", ""));
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
            return ret;
        }
        public void stop()
        {
            try
            {
                isWorking = false;
                if (mainThread != null)
                {
                    Global.WriteStatus("[Superodd] Program Stopped.");
                    mainThread.Abort();
                }
                if (marketThread != null)
                {
                    Global.WriteStatus("[Superodd] Upgrade Market Stopped.");
                    marketThread.Abort();
                }
                closeSocket();
                if (balanceThread != null)
                {
                    balanceThread.Abort();
                }
            }
            catch (Exception ex)
            {
                Global.WriteStatus(ex.ToString());
            }
        }
        public bool login()
        {
            InitLoginFormDatas();
            bool res = false;
            try
            {
                //if (loginStatus == 2)
                //    return res;
                //if (loginStatus == 1)
                //{
                //    res = true;
                //    return res;
                //}
                //loginStatus = 2;
                //if (getBalance() != -1)
                //{
                //    loginStatus = 1;
                //    res = true;
                //    return res;
                //}

                //httpClient.DefaultRequestHeaders.Remove("Expect");

                HttpResponseMessage httpLoginResponse = httpClient.PostAsync($"https://{Setting.Instance.domainSuperodd}/Defaults/Default1.aspx", new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("ScriptManager", "UpdatePanel|btnSignIn"),
                    new KeyValuePair<string, string>("__EVENTTARGET", ""),
                    new KeyValuePair<string, string>("__EVENTARGUMENT", ""),
                    new KeyValuePair<string, string>("__VIEWSTATE", strViewState),
                    new KeyValuePair<string, string>("__VIEWSTATEGENERATOR", strViewStateGenerator),
                    new KeyValuePair<string, string>("__VIEWSTATEENCRYPTED", ""),
                    new KeyValuePair<string, string>("__EVENTVALIDATION", strViewEnentValidation),
                    new KeyValuePair<string, string>("txtUserName", Setting.Instance.usernameSuperodd),
                    new KeyValuePair<string, string>("txtPassword", Setting.Instance.passwordSuperodd),
                    new KeyValuePair<string, string>("__ASYNCPOST", "true"),
                    new KeyValuePair<string, string>("btnSignIn", "")
                })).Result;

                string strLoginResponse = httpLoginResponse.Content.ReadAsStringAsync().Result;

                if (strLoginResponse.Contains("window.open('redirect.aspx"))
                {
                    loginStatus = 1;
                    Global.WriteStatus($"[Superodd] Login successed");
                    checkTime = DateTime.Now;
                    res = true;
                }
                else
                {
                    loginStatus = 0;
                    return false;
                }
            }
            catch (Exception e)
            {

            }
            return res;
        }
        public AddSlipResult addSlip(BetData betData)
        {
            AddSlipResult res = new AddSlipResult();
            res.bSuccess = false;
            res.dOdds = betData.dOdds;
            res.code = -1;
            try
            {
                Global.WriteStatus($"[Superodd] Add betslip - {betData.eventName} - {betData.strMarketName}/{betData.dOdds}/{betData.dStake}");
                if (!login())
                {
                    res.strMessage = $"[Superodd] Login failed";
                    res.code = -1;
                }
                else
                {
                    if (sendBetslipRequest(betData))
                    {
                        JArray slipArray = JArray.Parse(betData.slipContent);
                        if (slipArray.Count > 0)
                        {
                            double nowOdd = Utils.ParseToDouble(slipArray[7]?.ToString()) / 10;
                            res.dOdds = nowOdd;
                            betData.dOdds = nowOdd;
                            int oddStatus = BettingController.Instance.checkOdd(nowOdd, betData.dReverseOdds);
                            if (oddStatus == -1)
                            {
                                res.strMessage = $"Not profitable. Cancel bet";
                                res.code = -5;
                                return res;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            res.bSuccess = true;
            res.code = 0;
            return res;
        }
        public void removeSlip(BetData betData)
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }
        public bool sendBetslipRequest(BetData betData)
        {
            try
            {
                if (betData == null)
                    return false;
                Trace.WriteLine($"Start Betslip --> {betData.eventName} - {betData.strMarketName} - {betData.dOdds}");
                string sport = betData.period == 0 ? "fb" : "fb_ht";
                string reqBody = $"{{\"sport\":\"{sport}\",\"event_id\":\"{betData.eventId}\",\"bet_type\":\"{getBetType("soccer", betData)}\",\"equivalent_bets\":true,\"multiple_accounts\":false,\"betslip_type\":\"normal\",\"bookie_min_balances\":{{}}}}";
                HttpResponseMessage betslipMessage = httpClient.PostAsync($"https://{Setting.Instance.domainSuperodd}/v1/betslips/", new StringContent(reqBody, Encoding.UTF8, "application/json")).Result;
                string betslipContent = betslipMessage.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(betslipContent))
                    return false;
                if (Setting.Instance.isTest)
                    saveLog($"Betslip - {betslipContent}");
                //Trace.WriteLine($"Betslip - {betslipContent}");
                JObject obj = JObject.Parse(betslipContent);
                string status = obj["status"]?.ToString();
                if (status == "ok")
                {
                    string betslipId = obj["data"]["betslip_id"].ToString();
                    if (!string.IsNullOrEmpty(betslipId))
                    {
                        HttpResponseMessage slipDetailMsg = httpClient.GetAsync($"https://{Setting.Instance.domainSuperodd}/v1/betslips/{betslipId}").Result;
                        string slipDetail = slipDetailMsg.Content.ReadAsStringAsync().Result;
                        //Trace.WriteLine($"Betslip Detail - {slipDetail}");
                        List<double> priceList = new List<double>();
                        JObject detail = JObject.Parse(slipDetail);
                        JArray accounts = detail["data"]["accounts"] as JArray;
                        if (accounts != null && accounts.Count > 0)
                        {
                            foreach (JToken acc in accounts)
                            {
                                JArray pList = acc["price_list"] as JArray;
                                if (pList != null && pList.Count > 0)
                                {
                                    foreach (JToken p in pList)
                                    {
                                        priceList.Add((double)(p["effective"]["price"]));
                                    }
                                }
                            }
                        }
                        double maxPrice = priceList.Count > 0 ? priceList.Max() : 0;
                        Trace.WriteLine($"Betslip odds - ({detail["data"]["bet_type_description"]?.ToString()}) - {maxPrice}");
                        if (Setting.Instance.isTest)
                            saveLog($"Betslip Detail - {slipDetail}");
                    }
                }
                
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public BetResult confirmBet(BetData betData)
        {
            BetResult res = new BetResult();
            try
            {
                if (!string.IsNullOrEmpty(betData.slipContent))
                {
                    string bettingContent = sendConfirmRequest(betData);
                    res = getBetResult(bettingContent);
                    if (res.bSuccess)
                    {
                        betData.bettingId = res.bettingId;
                        betData.status = 1;
                        if (res.odd > 0)
                            betData.dOdds = res.odd;
                    }
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            if (res.bSuccess)
            {
                getBalance();
                Global.BalanceUpdate();
            }
            return res;
        }
        private string sendConfirmRequest(BetData betData)
        {
            string bettingContent = string.Empty;
            try
            {
                int count = 0;
                JArray slipArray = JArray.Parse(betData.slipContent);
                string tmpUrl = slipArray[48]?.ToString();
                while (count < 2)
                {
                    string betUrl = $"https://{Setting.Instance.domainSuperodd}/_Bet/{tmpUrl}&amt={betData.dStake}";
                    HttpResponseMessage betMessage = httpClient.PostAsync(betUrl, null).Result;
                    bettingContent = betMessage.Content.ReadAsStringAsync().Result;
                    if (Setting.Instance.isTest)
                        saveLog($"Betting - {bettingContent}");
                    if (!bettingContent.StartsWith("CHG|"))
                        break;
                    string strOdd = Regex.Match(bettingContent, @"</span>!\|(?<VAL>[^|]*)").Groups["VAL"].Value;
                    tmpUrl = Regex.Replace(tmpUrl, @"odds=[^&]*", $"odds={strOdd}");
                    count++;
                }
                Trace.WriteLine($"Superodd Betting result - {bettingContent}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"sendBetRequest error - {ex.Message}");
            }
            return bettingContent;
        }
        private string getMarketType(BetData data)
        {
            string type = string.Empty;
            if (data.nType == 0)    //Head to Head
            {
                return data.nSide == 0 ? "1" : "2";
            }
            else if (data.nType == 1)
            {
                return data.nSide == 0 ? "home" : "away";
            }
            else if (data.nType == 2)
            {
                return data.nSide == 0 ? "over" : "under";
            }
            return type;
        }
        private BetResult getBetResult(string betResponse)
        {
            BetResult ret = new BetResult();
            try
            {
                if (string.IsNullOrEmpty(betResponse))
                    return ret;
                if (betResponse.Contains("successful"))
                {
                    ret.bSuccess = true;
                    ret.code = ret.bSuccess ? 0 : -6;
                    ret.bettingId = Regex.Match(betResponse, @"\|r=(?<VAL>[^|]*)").Groups["VAL"].Value;
                    ret.strMessage = "Successfully bet finished";
                    ret.odd = Utils.ParseToDouble(Regex.Match(betResponse, "@ (?<VAL>[^\n]*)").Groups["VAL"].Value);
                    return ret;
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            return ret;
        }
        public double getBalance()
        {
            double balance = -1;
            try
            {
                httpClient.DefaultRequestHeaders.Remove("session");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("session", session_id);
                string balanceUrl = $"https://{Setting.Instance.domainSuperodd}/v1/customers/{username}/accounting_info/";
                HttpResponseMessage response = httpClient.GetAsync(balanceUrl).Result;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    loginStatus = 0;
                    return -1;
                }
                string result = response.Content.ReadAsStringAsync().Result;
                dynamic obj = JObject.Parse(result);
                foreach (dynamic balance_obj in obj["data"])
                {
                    if (balance_obj.key.ToString() == "available_credit")
                    {
                        balance = (double)balance_obj.value;
                        break;
                    }
                }
                balanceTime = DateTime.Now;
                Setting.Instance.balanceSuperodd = balance;
                Global.BalanceUpdate();
                return balance;
            }
            catch (Exception e)
            {

            }
            return balance;
        }
        private void saveLog(string content)
        {
            string dir = $"{Application.StartupPath}\\packet";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string path = $"{dir}\\Superodd_{DateTime.Now.ToString("yyyyMMdd")}.txt";
            using (FileStream fs = File.Open(path, FileMode.Append, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                    writer.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] {content}");
            }
        }
        public string getBetType(string sport, BetData data)
        {
            string ret = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(sport) || data == null)
                    return ret;
                string prefix = string.Empty;
                int handNumber = (int)(Utils.ParseToDouble(data.strHandicap) * 4);
                //if (data.nType == 0 && data.nSide == 1)
                //    handNumber = -handNumber;
                switch (sport)
                {
                    case "soccer":
                        if (data.nType == 0)        //Goal Handicap
                            ret = ret = $"for,ah,{(data.nSide == 0 ? "h" : "a")},{handNumber}";
                        else if (data.nType == 1)   //Over Under
                            ret = $"for,ah{(data.nSide == 0 ? "over" : "under")},{handNumber}";
                        break;
                    //case "tennis":
                    //    if (marketName.Contains("OVER"))
                    //    {
                    //        ret = $"for,tset,all,vwhatever,game,ahover,{handNumber}";
                    //    }
                    //    else if (marketName.Contains("UNDER"))
                    //    {
                    //        ret = $"for,tset,all,vwhatever,game,ahunder,{handNumber}";
                    //    }
                    //    else
                    //    {
                    //        prefix = marketName.Contains(homeTeam) ? "1" : "2";
                    //        if (prefix == "2")
                    //            handNumber *= -1;
                    //        if (hasLine)
                    //            ret = $"for,tset,all,vwhatever,game,ah,p{prefix},{handNumber}";
                    //        else
                    //        {
                    //            if (marketName.Contains("1st Set"))
                    //                ret = $"for,tset,1,vwhatever,p{prefix}";
                    //            else if (marketName.Contains("2nd Set"))
                    //                ret = $"for,tset,2,vwhatever,p{prefix}";
                    //            else
                    //                ret = $"for,tset,all,vwhatever,p{prefix}";
                    //        }
                    //    }
                    //    break;
                    //case "basket":
                    //    if (marketName.Contains("OVER"))
                    //    {
                    //        ret = $"for,ahover,{handNumber}";
                    //    }
                    //    else if (marketName.Contains("UNDER"))
                    //    {
                    //        ret = $"for,ahunder,{handNumber}";
                    //    }
                    //    else
                    //    {
                    //        prefix = marketName.Contains(homeTeam) ? "h" : "a";
                    //        if (prefix == "a")
                    //            handNumber *= -1;
                    //        if (hasLine)
                    //            ret = $"for,ah,{prefix},{handNumber}";
                    //        else
                    //            ret = $"for,ml,{prefix}";
                    //    }
                    //    break;
                    //case "mma":         //// New Sports : volleyball, mma, baseball, ice hockey
                    //    if (marketName.Contains("OVER"))
                    //    {
                    //        ret = $"for,ahover,{handNumber}";
                    //    }
                    //    else if (marketName.Contains("UNDER"))
                    //    {
                    //        ret = $"for,ahunder,{handNumber}";
                    //    }
                    //    else
                    //    {
                    //        prefix = marketName.Contains(homeTeam) ? "h" : (marketName.Contains(awayTeam) ? "a" : "d");
                    //        if (prefix == "a")
                    //            handNumber *= -1;
                    //        if (hasLine)
                    //            ret = $"for,ah,{prefix},{handNumber}";
                    //        else
                    //            ret = $"for,ml,{prefix}";
                    //    }
                    //    break;
                    //case "baseball":
                    //case "ice hockey":
                    //    if (marketName.Contains("OVER"))
                    //    {
                    //        ret = $"for,tp,all,ahover,{handNumber}";
                    //    }
                    //    else if (marketName.Contains("UNDER"))
                    //    {
                    //        ret = $"for,tp,all,ahunder,{handNumber}";
                    //    }
                    //    else
                    //    {
                    //        prefix = marketName.Contains(homeTeam) ? "h" : "a";
                    //        if (prefix == "a")
                    //            handNumber *= -1;
                    //        if (hasLine)
                    //            ret = $"for,tp,all,ah,{prefix},{handNumber}";
                    //        else
                    //            ret = $"for,tp,all,ml,{prefix}";
                    //    }
                    //    break;
                }

            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            return ret;
        }
        #region Socket
        public void checkSocket()
        {
            try
            {
                if (loginStatus == 1 && (webSocket == null || socketStatus == 0))
                {
                    Global.WriteStatus($"[Superodd] Check Socket and Try connect again!");
                    doConnect();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
        }
        public void doConnect()
        {
            try
            {
                socketStatus = 1;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                webSocket = new WebSocket(string.Format("wss://pro.drinbet.com/cpricefeed/?token={0}&lang=en&prices_bookies=pin,isn,ibc,pin88", session_id));
                //webSocket.SetProxy("http://127.0.0.1:8888", "", "");
                webSocket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                webSocket.Origin = $"https://{Setting.Instance.domainSuperodd}";
                webSocket.OnOpen += Socket_OnOpen;
                webSocket.OnMessage += Socket_OnMessage;
                webSocket.OnClose += Socket_OnClose;
                webSocket.OnError += Socket_OnError;
                webSocket.Compression = CompressionMethod.None;
                webSocket.EmitOnPing = true;
                webSocket.Connect();
                Task.Run(sendPing);
                //while (true)
                //{
                //    if (_webSocket != null && _webSocket.ReadyState != WebSocketState.Open)
                //    {
                //        Thread.Sleep(1000);
                //        break;
                //    }
                //    Thread.Sleep(100);
                //}

            }
            catch { }
        }
        public void closeSocket()
        {
            try
            {
                Trace.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] Closing Socket!");
                if (webSocket != null)
                    webSocket.Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
        }
        private void Socket_OnOpen(object sender, EventArgs e)
        {
            long timeStamp = Utils.getTick();
            webSocket.Send($"[\"ping\", \"{timeStamp.ToString()}\"]");
            Global.WriteStatus("[Superodd] Socket_OnOpen");
            socketStatus = 2;
        }

        private void Socket_OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {

            try
            {
                if (e.Data.ToString() == "3probe")
                {
                    webSocket.Send("5");
                    return;
                }
                parseMessage(e.Data);
            }
            catch (Exception ex)
            {
                //m_handlerWriteStatus("Exception in socket message: " + ex.ToString());
            }
        }

        private void Socket_OnClose(object sender, CloseEventArgs e)
        {
            socketStatus = 0;
            Global.WriteStatus($"[Superodd] Socket_OnClose - {e.Reason}");
            doConnect();
        }

        private void Socket_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            socketStatus = 0;
            Global.WriteStatus($"[Superodd] Socket_OnError - {e.Message}");
        }

        public void SendMessage(string message)
        {
            try
            {
                if (webSocket != null && webSocket.ReadyState == WebSocketState.Open)
                    webSocket.Send(message);
            }
            catch { }
        }

        private void sendPing()
        {
            if (webSocket != null && webSocket.ReadyState == WebSocketState.Open)
            {
                long timeStamp = Utils.getTick();
                webSocket.Send($"[\"ping\", \"{timeStamp.ToString()}\"]");
            }
        }

        private void parseMessage(string message)
        {
            try
            {
                if (message.Contains("pong"))
                {
                    Task.Run(() =>
                    {
                        Thread.Sleep(2500);
                        sendPing();
                    });
                }
                else
                {
                    JArray arrObj = JArray.Parse(message);
                    foreach (JToken one in arrObj)
                    {
                        try
                        {
                            string type = one[0].ToString();
                            if (type == "event")
                            {
                                JArray eventObj = (JArray)one;
                                if (eventObj.Count < 2)
                                    continue;
                                string sport = eventObj[1][0].ToString();
                                if (sport == "fb" || sport == "fb_ht")
                                {
                                    bool isLive = eventObj[2]["ir_status"] != null;
                                    DateTime startTime = Utils.ParseToDateTime(eventObj[2]["start_ts"].ToString());
                                    DateTime utcNow = DateTime.UtcNow;
                                    DateTime utcLimit = utcNow.AddHours(limitHours);
                                    if (!isLive && startTime > utcNow && startTime < utcLimit)
                                    {
                                        MatchItem match = new MatchItem();
                                        match.matchId = eventObj[1][1].ToString();
                                        match.leagueId = eventObj[2]["competition_id"].ToString();
                                        match.leagueName = eventObj[2]["competition_name"].ToString();
                                        match.home = eventObj[2]["home"].ToString();
                                        match.away = eventObj[2]["away"].ToString();
                                        match.startTime = startTime;
                                        if (matches.Find(o => o.matchId == match.matchId) == null)
                                        {
                                            matches.Add(match);
                                            RelationCtrl.Instance.updateMatch("MB", match);
                                        }
                                        webSocket.Send($"[\"watch_hcaps\",[[{match.leagueId},\"{sport}\",\"{match.matchId}\"]]]");
                                    }
                                }
                            }
                            else if (type == "offers_hcap")
                            {
                                JArray handObj = (JArray)one;
                                string sport = handObj[1][1].ToString();
                                string matchId = handObj[1][2].ToString();
                                if (sport == "fb" || sport == "fb_ht")
                                {
                                    int period = sport == "fb_ht" ? 1 : 0;
                                    MatchItem curMatch = matches.Find(o => o.matchId == matchId);
                                    if (curMatch != null)
                                    {
                                        List<Market> markets = parseMarketContent((JObject)(handObj[2]), matchId, period);
                                        if (markets.Count > 0)
                                        {
                                            foreach (Market market in markets)
                                                RelationCtrl.Instance.updateMarket("MB", market);
                                        }
                                    }
                                }
                            }
                        }
                        catch { }
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
