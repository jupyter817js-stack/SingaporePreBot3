using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using WebSocketSharp;

namespace SingaporePreBot3
{
    public class BFCtrl : ISiteController
    {
        private CookieContainer cookieContainer = null;
        private HttpClient httpClient = null;
        public bool isWorking = false;
        Thread mainThread = null;
        int loginStatus;
        long counter = Utils.getTick();
        Random rnd = new Random();
        DateTime balanceTime = DateTime.Now;
        DateTime checkTime = DateTime.Now;
        string session_id;
        string username;
        long tick = 0;
        WebSocket webSocket = null;
        int socketStatus = 0;
        List<MatchItem> matches = new List<MatchItem>();
        int limitHours = 24;
        public BFCtrl()
        {
        }

        public void start()
        {
            try
            {
                if (mainThread == null || !mainThread.IsAlive)
                {
                    isWorking = true;
                    Global.WriteStatus("[Betfair] Program Started.");
                    mainThread = new Thread(() => run());
                    mainThread.Start();
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
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
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
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Setting.Instance.userAgent);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9,ko;q=0.8");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
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
            }
            catch (Exception ex)
            {
                Global.WriteStatus(ex.ToString());
            }
        }
        public bool login()
        {
            bool res = false;
            try
            {
                if (loginStatus == 2)
                    return res;
                if (loginStatus == 1)
                {
                    res = true;
                    return res;
                }
                loginStatus = 2;
                if (getBalance() != -1)
                {
                    loginStatus = 1;
                    res = true;
                    return res;
                }

                httpClient.DefaultRequestHeaders.Remove("session");
                httpClient.DefaultRequestHeaders.Remove("authorization");
                httpClient.DefaultRequestHeaders.Remove("Referer");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", $"https://{Setting.Instance.domainSuperodd}/login?next=/trade");
                string reqBody = $"{{\"username\":\"{Setting.Instance.usernameSuperodd}\",\"password\":\"{Setting.Instance.passwordSuperodd}\",\"lang\":\"en\"}}";
                HttpResponseMessage respMsg = httpClient.PostAsync($"https://{Setting.Instance.domainSuperodd}/web/sessions/", new StringContent(reqBody, Encoding.UTF8, "application/json")).Result;
                string respBody = respMsg.Content.ReadAsStringAsync().Result;
                JObject obj = JObject.Parse(respBody);
                if (obj["data"]["username"]?.ToString().ToLower() == Setting.Instance.usernameSuperodd.Trim().ToLower())
                {
                    loginStatus = 1;
                    session_id = obj["data"]["session_id"].ToString();
                    username = obj["data"]["username"].ToString();
                    Global.WriteStatus($"[Superodd] Login successed");
                    checkTime = DateTime.Now;
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
            catch (Exception ex)
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
    }
}
