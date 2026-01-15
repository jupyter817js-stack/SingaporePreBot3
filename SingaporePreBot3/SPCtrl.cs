using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SingaporePreBot3
{
    public class SPCtrl : ISiteController
    {
        private HttpClient httpClient = null;
        Thread mainThread = null;
        Thread refrehThread = null;
        bool isWorking = false;
        int loginStatus;
        public string authToken;
        public string metaData;
        public string sessionId;
        int refreshMin = 5;
        DateTime refrehTime = DateTime.Now;
        public SPCtrl()
        {
        }

        public void start()
        {
            try
            {
                if(mainThread == null || !mainThread.IsAlive)
                {
                    isWorking = true;
                    Global.WriteStatus("[SP] Program Started.");
                    mainThread = new Thread(() => run());
                    mainThread.Start();
                    refrehThread = new Thread(refreshWork);
                    refrehThread.Start();
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }
        private void initBrowser()
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"initBrowser error-{ex.Message}");
            }
        }
        private void refreshWork()
        {
            try
            {
                while (isWorking)
                {
                    Thread.Sleep(60000);
                    Global.KeepLogin();
                    if (refrehTime.AddMinutes(refreshMin) < DateTime.Now)
                    {
                        Global.RefreshBrowser();
                        Global.GetCookie();
                        refrehTime = DateTime.Now;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
        }
        public void getCookie()
        {
            try
            {
                Global.GetCookie();
                initHttpClient();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
        }
        private void run()
        {
            try
            {
                getCookie();
                getBalance();
                while (true)
                {
                    //if (Setting.Instance.isLicense)
                        getPrematchEvents();
                    Thread.Sleep(60000 * Setting.Instance.singaporeTime);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
        }

        public void initHttpClient()
        {
            var useAuth = false;
            //proxyItem = new WebProxy("104.131.82.181:22225");
            HttpClientHandler handler = new HttpClientHandler()
            {
                //Proxy = proxyItem,
                //UseProxy = true,
                //PreAuthenticate = useAuth,
                //UseDefaultCredentials = !useAuth,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            handler.CookieContainer = Setting.Instance.spContainer;
            httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Setting.Instance.userAgent);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9,ko;q=0.8");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-mfp-analytics-metadata", metaData);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-session-id", sessionId);
            httpClient.DefaultRequestHeaders.Add("Authorization", authToken);
            httpClient.DefaultRequestHeaders.Add("Referer", $"https://{Setting.Instance.domainSingapore}/en/sports/category/1/football");
        }
        public List<MatchItem> parseEventContent(string content)
        {
            List<MatchItem> ret = new List<MatchItem>();
            if (string.IsNullOrEmpty(content))
                return ret;
            try
            {
                dynamic jsonData = JsonConvert.DeserializeObject<dynamic>(content);
                if (jsonData != null && jsonData["events"] != null)
                {
                    foreach (dynamic eventOne in jsonData["events"])
                    {
                        try
                        {
                            string sportId = eventOne["type"]["sportClass"]["category"]["id"]?.ToString();
                            if (sportId != "1")
                                continue;
                            MatchItem newMatch = getMatch(eventOne);
                            if (newMatch != null)
                            {
                                newMatch.markets = getMarket(newMatch.matchId, eventOne["markets"]);
                                ret.Add(newMatch);
                            }
                                
                        }
                        catch (Exception ex)
                        {
                            string error = ex.Message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return ret;
        }
        private MatchItem getMatch(dynamic eventOne)
        {
            MatchItem newMatch = null;
            try
            {
                newMatch = new MatchItem();
                DateTime dateTime = Utils.ParseToDateTime(eventOne["startTime"]?.ToString());
                newMatch.startTime = dateTime;
                //newMatch.startTime = dateTime.AddHours(8);
                //if ((newMatch.startTime.Day != DateTime.Now.Day) && (newMatch.startTime.Day != DateTime.Now.AddDays(1).Day))
                //{
                //newMatch = null;
                //return newMatch;
                //}
                //else
                {
                    newMatch.leagueId = eventOne["type"]["id"]?.ToString();
                    string strLeague = eventOne["type"]["name"]?.ToString();
                    if (!string.IsNullOrEmpty(strLeague))
                        newMatch.leagueName = strLeague.Replace("(Live)", "").Trim();
                    string eventName = eventOne["name"]?.ToString();
                    newMatch.matchId = eventOne["id"]?.ToString();

                    if (!string.IsNullOrEmpty(eventName))
                    {
                        string[] tmp = eventName.Replace("(Live)", "").Trim().Split(new string[] { " vs " }, StringSplitOptions.None);
                        if (tmp.Length == 2)
                        {
                            newMatch.home = tmp[0];
                            newMatch.away = tmp[1];
                        }
                    }
                    newMatch.updateTime = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
            return newMatch;
        }
        public int getPBTime(string round, string min)
        {
            int ret = 0;
            string strMin = Regex.Match(min, @"(?<VAL>[\d]*)").Groups["VAL"].Value;
            if (round == "1H")
            {
                ret = Utils.ParseToInt(strMin);
            }
            else if (round == "2H")
            {
                ret = 45 + Utils.ParseToInt(strMin);
            }
            else if (round == "HT")
            {
                ret = 45;
            }
            return ret;
        }
        private List<Market> getMarket(string matchId, dynamic marketItem)
        {
            List<Market> markets = new List<Market>();
            try
            {
                foreach (dynamic marketOne in marketItem)
                {
                    string periodId = marketOne["id"]?.ToString();
                    string name = marketOne["name"]?.ToString();
                    string marketStatusCode = marketOne["marketStatusCode"]?.ToString();
                    if (!isValidMarket(name, marketStatusCode))
                        continue;

                    if (name.Contains("Halftime") || name.Contains("Half Time"))
                        continue;
                        //newItem.period = 1;
                    Market newItem = new Market();
                    newItem.matchId = matchId;
                    newItem.marketId = marketOne["id"]?.ToString();
                    if (name.Contains("Asian Handicap"))
                        newItem.nType = 0;
                    else if (name.Contains("Over/Under"))
                        newItem.nType = 1;
                    //newItem.matchId = marketOne["eventId"]?.ToString();
                    string strLine = "";
                    if (newItem.nType == 0)
                        strLine = (Utils.ParseToDouble(marketOne["handicapValue"]?.ToString()) / Utils.ParseToDouble(marketOne["maxAccumulator"]?.ToString())).ToString();
                    else if (newItem.nType == 1)
                        strLine = marketOne["handicapValue"]?.ToString();
                    newItem.strLine = strLine;
                    newItem.dOdd1 = Utils.ParseToDouble(marketOne["outcomes"][0]["prices"][0]["decimal"]?.ToString());
                    newItem.dOdd2 = Utils.ParseToDouble(marketOne["outcomes"][1]["prices"][0]["decimal"]?.ToString());
                    newItem.outcome1 = marketOne["outcomes"][0]["id"]?.ToString();
                    newItem.outcome2 = marketOne["outcomes"][1]["id"]?.ToString();
                    newItem.status = marketStatusCode;
                    newItem.stamp = Utils.getTick();
                    markets.Add(newItem);
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            return markets;
        }
        public bool isValidMarket(string marketType, string periodId)
        {
            if (string.IsNullOrEmpty(marketType) || marketType.Contains("1X2") || marketType.Contains("PTS") || marketType.Contains("Score") || marketType.Contains("Odd/Even"))
                return false;
            if (marketType.Contains("Total Goals Over/Under") || marketType.Contains("1/2 Goal"))
                return true;
            if (marketType.Contains("Total Goals"))
                return false;
            Trace.WriteLine($"New market type - {marketType}");
            if (periodId == "A")
                return true;
            Trace.WriteLine($"New status type - {periodId}");
            return false;
        }
        public void stop()
        {
            try
            {
                isWorking = false;
                if (mainThread != null)
                {
                    Global.WriteStatus("[SP] Program Stopped.");
                    mainThread.Abort();
                }
                if (mainThread != null)
                {
                    mainThread.Abort();
                }
                if (refrehThread != null)
                {
                    refrehThread.Abort();
                }
            }
            catch (Exception ex)
            {
                Global.WriteStatus(ex.ToString());
            }
        }
        public bool login()     //Return login status
        {
            // Simulate a login process and set the loginStatus field
            loginStatus = PerformLogin() ? 1 : 0;
            return loginStatus == 1;
        }

        // Add a helper method to simulate the login process
        private bool PerformLogin()
        {
            // Replace this with actual login logic
            return true; // Assume login is successful for now
        }
        public void getLiveEvents()
        {
            try
            {
                httpClient.DefaultRequestHeaders.Referrer = new Uri($"https://{Setting.Instance.domainSingapore}/en/sports/live-betting");
                httpClient.DefaultRequestHeaders.Remove("x-wl-analytics-tracking-id");
                httpClient.DefaultRequestHeaders.Add("x-wl-analytics-tracking-id", Guid.NewGuid().ToString());
                HttpResponseMessage resp = httpClient.GetAsync($"https://{Setting.Instance.domainSingapore}/mfp/api/adapters/spplMfpApi/event/live?lang=en&categoryName=football&mode=web").Result;
                if (resp.StatusCode != HttpStatusCode.OK)
                {
                    authToken = Global.GetAuthToken();
                    httpClient.DefaultRequestHeaders.Remove("Authorization");
                    httpClient.DefaultRequestHeaders.Add("Authorization", authToken);
                    httpClient.DefaultRequestHeaders.Remove("x-wl-analytics-tracking-id");
                    httpClient.DefaultRequestHeaders.Add("x-wl-analytics-tracking-id", Guid.NewGuid().ToString());
                    resp = httpClient.GetAsync($"https://{Setting.Instance.domainSingapore}/mfp/api/adapters/spplMfpApi/event/live?lang=en&categoryName=football&mode=web").Result;
                }
                string liveContent = resp.Content.ReadAsStringAsync().Result;
                List<MatchItem> matches = parseEventContent(liveContent);
                RelationCtrl.Instance.setMatches("SP", matches);
                Global.UpdateSingapore();
                //if (newMatches != null)
                //{
                //    //Trace.WriteLine($"PB data - {JsonConvert.SerializeObject(newMatches)}");
                //    foreach (MatchItem one in newMatches)
                //        RelationCtrl.Instance.updateMatch("DF", one);
                    
                //    //matches = newMatches;
                //}
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
        }
        public void getPrematchEvents()
        {
            try
            {
                httpClient.DefaultRequestHeaders.Referrer = new Uri($"https://{Setting.Instance.domainSingapore}en/sports/category/1/football");
                authToken = Global.GetAuthToken();
                httpClient.DefaultRequestHeaders.Remove("Authorization");
                httpClient.DefaultRequestHeaders.Add("Authorization", authToken);
                httpClient.DefaultRequestHeaders.Remove("x-wl-analytics-tracking-id");
                httpClient.DefaultRequestHeaders.Add("x-wl-analytics-tracking-id", Guid.NewGuid().ToString());
                
                HttpResponseMessage respOverUnder = httpClient.GetAsync($"https://{Setting.Instance.domainSingapore}/mfp/api/adapters/spplMfpApi/event/upcoming/football?lang=en&betType=HL").Result;       //  Over / Under
                string ouContent = respOverUnder.Content.ReadAsStringAsync().Result;
                List<MatchItem> matches1 = parseEventContent(ouContent);

                HttpResponseMessage respAH = httpClient.GetAsync($"https://{Setting.Instance.domainSingapore}/mfp/api/adapters/spplMfpApi/event/upcoming/football?lang=en&betType=AH").Result;       //  Asian Handicap
                string ahContent = respAH.Content.ReadAsStringAsync().Result;
                List<MatchItem> matches2 = parseEventContent(ahContent);
                foreach (MatchItem one in matches2)
                {
                    MatchItem curMatch = matches1.Find(o => o.matchId == one.matchId);
                    if (curMatch == null)
                        matches1.Add(one);
                    else
                        curMatch.markets.AddRange(one.markets);
                }

                HttpResponseMessage respMR = httpClient.GetAsync($"https://{Setting.Instance.domainSingapore}/mfp/api/adapters/spplMfpApi/event/upcoming/football?lang=en&betType=MR").Result;       //  1X2
                string mrContent = respMR.Content.ReadAsStringAsync().Result;
                List<MatchItem> matchesMR = parseEventContent(mrContent);
                foreach (MatchItem one in matchesMR)
                {
                    MatchItem curMatch = matches1.Find(o => o.matchId == one.matchId);
                    if (curMatch == null)
                        matches1.Add(one);
                    else
                        curMatch.markets.AddRange(one.markets);
                }
                RelationCtrl.Instance.setMatches("SP", matches1);
                Global.UpdateSingapore();
                RelationCtrl.Instance.checkSiteCandidate("SP");
                //bettype HL:Over/Under  WH:1/2 Goal
                //
                //if (newMatches != null)
                //{
                //    //Trace.WriteLine($"PB data - {JsonConvert.SerializeObject(newMatches)}");
                //    foreach (MatchItem one in newMatches)
                //        RelationCtrl.Instance.updateMatch("DF", one);

                //    //matches = newMatches;
                //}
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
        }
        public double getBalance()
        {
            double balance = -1;
            try
            {
                string balanceUrl = $"https://{Setting.Instance.domainSingapore}/mfp/api/adapters/spplMfpApi/accounts/balance";
                HttpResponseMessage response = httpClient.GetAsync(balanceUrl).Result;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    authToken = Global.GetAuthToken();
                    httpClient.DefaultRequestHeaders.Remove("Authorization");
                    httpClient.DefaultRequestHeaders.Add("Authorization", authToken);
                    httpClient.DefaultRequestHeaders.Remove("x-wl-analytics-tracking-id");
                    httpClient.DefaultRequestHeaders.Add("x-wl-analytics-tracking-id", Guid.NewGuid().ToString());
                }    
                string result = response.Content.ReadAsStringAsync().Result;
                if (result.Equals("error"))
                {
                    return -1;
                }
                JObject balanceObj = JObject.Parse(result);
                balance = Utils.ParseToDouble(balanceObj["balance"].ToString());
                Setting.Instance.balanceSingapore = balance;
                Global.BalanceUpdate();
                return balance;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
            return balance;
        }

        public bool checkOdds()
        {
            bool bSame = false;
            try
            {

            }
            catch (Exception e) { }
            return bSame;
        }
        public AddSlipResult addSlip(BetData betData)
        {       
            AddSlipResult res = new AddSlipResult();
            res.bSuccess = false;
            res.dOdds = betData.dOdds;
            res.code = -1;
            try
            {
                Global.WriteStatus($"[SP] Start betting - {betData.eventName} - {betData.strMarketName}/{betData.dOdds}/{betData.dStake}");
                bool slipResult = addBetslip(betData);
                if (slipResult)
                {
                    JObject betSlipObj = JObject.Parse(betData.slipContent);
                    int failCount = betSlipObj["betFailures"]?.Count() ?? 0;
                    if (failCount > 0)
                    {
                        res.strMessage = betSlipObj["betFailures"][0]["betErrors"]?[0]["betFailureReason"]?.ToString();
                        res.code = -4;
                        return res;
                    }
                    double maxStake = Utils.ParseToDouble(betSlipObj["bets"][0]["betMaxStake"]?.ToString());
                    double nowOdd = Utils.ParseToDouble(betSlipObj["bets"][0]["betPrice"]?.ToString());
                    res.dOdds = nowOdd;
                    betData.dOdds = nowOdd;
                    //if (maxStake < betData.dStake)
                    //{
                    //    res.strMessage = $"Max stake is low than {betData.dStake}";
                    //    res.code = -6;
                    //    return res;
                    //}
                    res.bSuccess = true;
                    res.code = 0;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
            return res;
        }
        public bool addBetslip(BetData betData)
        {
            try
            {
                if (betData == null)
                    return false;
                int count = 0;
                string betSlipRespContent = string.Empty;
                string slipContent = $"[{{\"betNo\":1,\"legs\":[{{\"legNo\":1,\"legType\":\"W\",\"legSort\":\"--\",\"parts\":[{{\"outcome\":\"{betData.sectionId}\"}}]}}]}}]";
                HttpResponseMessage betslipRespMessage = null;
                while (++count < 4)
                {
                    httpClient.DefaultRequestHeaders.Remove("x-wl-analytics-tracking-id");
                    httpClient.DefaultRequestHeaders.Add("x-wl-analytics-tracking-id", Guid.NewGuid().ToString());
                    betslipRespMessage = httpClient.PostAsync($"https://{Setting.Instance.domainSingapore}/mfp/api/adapters/spplMfpApi/sports/bet/build", new StringContent(slipContent, Encoding.UTF8, "application/json")).Result;
                    if (betslipRespMessage.StatusCode == HttpStatusCode.OK)
                    {
                        betSlipRespContent = betslipRespMessage.Content.ReadAsStringAsync().Result;
                        break;
                    }
                    authToken = Global.GetAuthToken();
                    httpClient.DefaultRequestHeaders.Remove("Authorization");
                    httpClient.DefaultRequestHeaders.Add("Authorization", authToken);
                    httpClient.DefaultRequestHeaders.Add("Origin", "https://online.singaporepools.com");
                }
                if (!string.IsNullOrEmpty(betSlipRespContent))
                {
                    try
                    {
                        var json = JObject.Parse(betSlipRespContent);
                        var hasError = json["betFailures"] != null && json["betFailures"].Any();
                        if (hasError)
                            return false;

                        betData.slipContent = betSlipRespContent;
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }

                return false;
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
                    string bettingContent = sendBetRequest(betData);
                    Trace.WriteLine($"Stake after sendBetRequest - {betData.dStake}");
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
                //getBalance();
                Global.BalanceUpdate();
            }
            Trace.WriteLine($"Stake return confirmBet - {betData.dStake}");
            return res;
        }
        private string sendBetRequest(BetData betData)
        {
            string bettingContent = string.Empty;
            try
            {
                int count = 0;
                JObject betSlipObject = JObject.Parse(betData.slipContent);
                int priceDen = Utils.ParseToInt(betSlipObject["outcomeDetails"][0]["priceDen"]?.ToString());
                int priceNum = Utils.ParseToInt(betSlipObject["outcomeDetails"][0]["priceNum"]?.ToString());
                //string placeContent = $"{{\"remoteUniqueId\":\"W:{Guid.NewGuid()}\",\"sportsBet\":[{{\"numLines\":1,\"betNo\":1,\"betType\":\"SGL\",\"stakePerLine\":\"{betData.dStake}\",\"legType\":\"W\",\"legs\":[{{\"legNo\":1,\"legSort\":\"--\",\"parts\":[{{\"outcome\":\"{betData.sectionId}\",\"priceType\":\"L\",\"priceNum\":\"{priceNum}\",\"priceDen\":\"100\"}}]}}]}}]}}";
                string placeContent = $"{{\"remoteUniqueId\":\"W:{Guid.NewGuid()}\",\"sportsBet\":[{{\"numLines\":1,\"betNo\":1,\"betType\":\"SGL\",\"stakePerLine\":\"{betData.dStake}\",\"legType\":\"W\",\"legs\":[{{\"legNo\":1,\"legSort\":\"--\",\"parts\":[{{\"outcome\":\"{betData.sectionId}\",\"priceType\":\"L\",\"priceNum\":\"{priceNum}\",\"priceDen\":\"{priceDen}\"}}]}}]}}]}}";
                HttpResponseMessage placeMessage = null;
                while (++count < 4)
                {
                    httpClient.DefaultRequestHeaders.Remove("x-wl-analytics-tracking-id");
                    httpClient.DefaultRequestHeaders.Add("x-wl-analytics-tracking-id", Guid.NewGuid().ToString());
                    placeMessage = httpClient.PostAsync($"https://{Setting.Instance.domainSingapore}/mfp/api/adapters/spplMfpApi/sports/bet/place", new StringContent(placeContent, Encoding.UTF8, "application/json")).Result;
                    if (placeMessage.StatusCode == HttpStatusCode.OK)
                    {
                        bettingContent = placeMessage.Content.ReadAsStringAsync().Result;
                        break;
                    }
                    authToken = Global.GetAuthToken();
                    httpClient.DefaultRequestHeaders.Remove("Authorization");
                    httpClient.DefaultRequestHeaders.Add("Authorization", authToken);
                    httpClient.DefaultRequestHeaders.Add("Origin", "https://online.singaporepools.com");
                }

                saveLog($"[Betting] - {bettingContent}");
                Trace.WriteLine($"[SP] Betting content - {bettingContent}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"${MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
            return bettingContent;
        }
        public BetResult getBetResult(string betContent)
        {
            BetResult ret = new BetResult();
            try
            {
                if (string.IsNullOrEmpty(betContent))
                    return ret;
                JObject placebetObj = JObject.Parse(betContent);
                int delayCount = placebetObj["betDelays"]?.Count() ?? 0;
                int failCount = placebetObj["betFailures"]?.Count() ?? 0;
                int placeCount = placebetObj["betPlacements"]?.Count() ?? 0;
                string slipId = placebetObj["slipId"]?.ToString();
                if (failCount > 0)
                {
                    ret.strMessage = placebetObj["betFailures"][0]["betErrors"]?[0]["betFailureReason"]?.ToString();
                }
                else if (!string.IsNullOrEmpty(slipId))
                {
                    //if (delayCount > 0)
                    //{
                        //int delay = Utils.ParseToInt(placebetObj["betDelays"][0]["delay"]?.ToString());
                        //Thread.Sleep(1000 * delay);
                        ret = getConfirmResult(slipId);
                    //}
                    //else if (placeCount > 0)
                    //{
                    //    string status = placebetObj["betPlacements"][0]["status"]?.ToString();
                    //    ret.bettingId = placebetObj["betPlacements"][0]["id"]?.ToString();
                    //    if (status == "P" || status == "A")
                    //    {
                    //        if (placebetObj["accountBalance"] != null)
                    //            Setting.Instance.balanceSingapore = Utils.ParseToDouble(placebetObj["accountBalance"]["balance"]?.ToString());
                    //        ret.bSuccess = true;
                    //        return ret;
                    //    }
                    //    else if (status == "ERROR")
                    //    {
                    //        ret.bSuccess = false;
                    //        ret.strMessage = placebetObj["betPlacements"][0]["legs"]?[0]["legFailureReason"]?.ToString();
                    //        return ret;
                    //    }
                    //}
                }
                ret.code = ret.bSuccess ? 0 : -6;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            return ret;
        }
        private BetResult getConfirmResult(string slipId)
        {
            BetResult ret = new BetResult();
            try
            {
                int count = 0;
                string content = string.Empty;
                int limit = 50;
                while (++count < limit)
                {
                    HttpResponseMessage slipMessage = httpClient.GetAsync($"https://{Setting.Instance.domainSingapore}/mfp/api/adapters/spplMfpApi/sports/betslip/{slipId}?lang=en").Result;
                    if (slipMessage.StatusCode != HttpStatusCode.OK)
                    {
                        authToken = Global.GetAuthToken();
                        httpClient.DefaultRequestHeaders.Remove("Authorization");
                        httpClient.DefaultRequestHeaders.Add("Authorization", authToken);
                        continue;
                    }

                    content = slipMessage.Content.ReadAsStringAsync().Result;
                    saveLog($"[Confirm] - {content}");

                    JObject newObj = JObject.Parse(content);
                    string status = newObj["bets"]?[0]?["status"]?.ToString();

                    if (status == "A")
                    {
                        if (newObj["accountBalance"] != null)
                            Setting.Instance.balanceSingapore = Utils.ParseToDouble(newObj["accountBalance"]["balance"]?.ToString());

                        ret.bSuccess = true;
                        ret.bettingId = newObj["bets"]?[0]?["id"]?.ToString();
                        return ret;
                    }
                    else if (status == "ERROR")
                    {
                        ret.bSuccess = false;
                        ret.strMessage = newObj["bets"]?[0]?["legs"]?[0]?["legFailureReason"]?.ToString();
                        ret.bettingId = newObj["bets"]?[0]?["id"]?.ToString();
                        return ret;
                    }
                    else
                    {
                        Trace.WriteLine($"[Confirm] Waiting... slipId={slipId}, status={status}");
                    }

                    Thread.Sleep(5000);
                }

                // 루프가 끝났는데도 상태가 결정되지 않은 경우
                ret.bSuccess = false;
                ret.strMessage = "Confirmation timeout or undetermined status";
            }
            catch (Exception ex)
            {
                ret.bSuccess = false;
                ret.strMessage = "Exception in getConfirmResult: " + ex.Message;
                Trace.WriteLine($"getConfirmResult error - {ex.Message}");
            }

            return ret;
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
        private void saveLog(string content)
        {
            string dir = $"{Application.StartupPath}\\packet";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string path = $"{dir}\\singapore_{DateTime.Now.ToString("yyyyMMdd")}.txt";
            using (FileStream fs = File.Open(path, FileMode.Append, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                    writer.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] {content}");
            }
        }
    }
}
