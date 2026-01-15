using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows.Forms;

namespace SingaporePreBot3
{
    public class Setting
    {
        private static Setting _instance = null;

        public static Setting Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Setting();
                }

                return _instance;
            }
        }
        public string programName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        public int candidateTime;
        public bool isTest;
        public string appuser { get; set; }
        public string domainSingapore { get; set; }
        public string usernameSingapore { get; set; }
        public string passwordSingapore { get; set; }
        public double balanceSingapore { get; set; }
        public string domainSuperodd { get; set; }
        public string usernameSuperodd { get; set; }
        public string passwordSuperodd { get; set; }
        public int startTime { get; set; }
        public int endTime { get; set; }
        public double balanceSuperodd { get; set; }
        public int singaporeTime { get; set; }
        public int superoddTime { get; set; }
        public string chromePath { get; set; }
        public int chromeSocketPort = 9583;
        public string chromeDir = $"{Application.StartupPath}\\chrome";
        public List<BetItem> betList = new List<BetItem>();
        public SPCtrl spCtrl = new SPCtrl();
        public MBCtrl mbCtrl = new MBCtrl();
        public BrowserCtrl browser = new BrowserCtrl();
        public List<PlaceBetCandidate> candidateList = new List<PlaceBetCandidate>();
        public string captchaKey = "d6ee2a96871a9050856c6a97c464e0a2";
        public int successWaitTime = 5;
        public List<BetOrder> orderList = new List<BetOrder>();
        public int roundType = 0;
        public double percentMax = 300;
        public double percentMin;
        public string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36";
        //public string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36";
        public CookieContainer spContainer = new CookieContainer();
        public List<StakeItem> stakeList = new List<StakeItem>();
        public List<string> blackLeagues = new List<string>();
        public bool playSound { get; set; }
        public bool isLicense;
        
        public string typeToken = "8407684332:AAFeRSxBm8wLrnS8QYZ03BjJGlBkxyZCDKs";

        public List<string> compareResult = new List<string>();
        public List<string> comparedMatchId = new List<string>();
        public string ReadRegistry(string KeyName)
        {
            return Registry.CurrentUser.CreateSubKey("SoftWare").CreateSubKey(programName).GetValue(KeyName, (object)"").ToString();
        }

        public void WriteRegistry(string KeyName, string KeyValue)
        {
            Registry.CurrentUser.CreateSubKey("SoftWare").CreateSubKey(programName).SetValue(KeyName, (object)KeyValue);
        }
        public void load()
        {
            domainSingapore = ReadRegistry("domainSingapore");
            usernameSingapore = ReadRegistry("usernameSingapore");
            passwordSingapore = ReadRegistry("passwordSingapore");
            domainSuperodd = ReadRegistry("domainSuperodd");
            usernameSuperodd = ReadRegistry("usernameSuperodd");
            passwordSuperodd = ReadRegistry("passwordSuperodd");
            startTime = Utils.ParseToInt(ReadRegistry("startTime"));
            endTime = Utils.ParseToInt(ReadRegistry("endTime"));
            string strStake = ReadRegistry("stakeList");
            if (!string.IsNullOrEmpty(strStake))
                stakeList = JsonConvert.DeserializeObject<List<StakeItem>>(strStake);
            singaporeTime = Utils.ParseToInt(ReadRegistry("singaporeTime"));
            if (singaporeTime == 0)
                singaporeTime = 1;
            superoddTime = Utils.ParseToInt(ReadRegistry("superoddTime"));
            if (superoddTime == 0)
                superoddTime = 1;
            candidateTime = Utils.ParseToInt(ReadRegistry("candidateTime"));
            if (candidateTime == 0)
                candidateTime = 1;
            percentMin = Utils.ParseToDouble(ReadRegistry("percentMin"));
            playSound = ReadRegistry("playSound") == "0" ? false : true;
            string strBlack = ReadRegistry("blackLeagues");
            if (!string.IsNullOrEmpty(strBlack))
                blackLeagues = JsonConvert.DeserializeObject<List<string>>(strBlack);
        }
        public void save()
        {
            WriteRegistry("domainSingapore", domainSingapore);
            WriteRegistry("usernameSingapore", usernameSingapore);
            WriteRegistry("passwordSingapore", passwordSingapore);
            WriteRegistry("domainSuperodd", domainSuperodd);
            WriteRegistry("usernameSuperodd", usernameSuperodd);
            WriteRegistry("passwordSuperodd", passwordSuperodd);
            WriteRegistry("startTime", startTime.ToString());
            WriteRegistry("endTime", endTime.ToString());
            WriteRegistry("stakeList", JsonConvert.SerializeObject(stakeList));
            WriteRegistry("singaporeTime", singaporeTime.ToString());
            WriteRegistry("superoddTime", superoddTime.ToString());
            WriteRegistry("candidateTime", candidateTime.ToString());
            WriteRegistry("percentMin", percentMin.ToString());
            WriteRegistry("playSound", playSound ? "1" : "0");
            WriteRegistry("blackLeagues", JsonConvert.SerializeObject(blackLeagues));
        }
        public PlaceBetCandidate getCandidate()
        {
            PlaceBetCandidate ret = null;
            try
            {
                IEnumerable<PlaceBetCandidate> candList = candidateList.Where(o => !o.isAlreadyExist());
                if (candList != null && candList.Count() > 0)
                    ret = candList.OrderByDescending(o => o.createTime).First();        //candList.OrderByDescending(o => o.dProfit).First();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Find candidate error - {ex.Message}");
            }
            return ret;
        }
        public List<BetOrder> getOrderList(bool display = false)
        {
            if (display)
                return orderList.Where(o => o.nOrderStatus == 1 || o.nOrderStatus == 2 || o.nOrderStatus == -1 || o.nOrderStatus == -2 || o.nOrderStatus == -4).ToList();
            else
                return orderList;
        }
        public double getStake(PlaceBetCandidate cand)
        {
            double ret = 0;
            try
            {
                if (stakeList.Count == 0)
                    return 0;
                int existCount = cand.getExistCount();
                StakeItem stake = stakeList.FindAll(o => o.min <= cand.dProfit && o.max >= cand.dProfit && o.count > existCount).OrderBy(o => o.count).FirstOrDefault();
                if (stake != null)
                    ret = stake.stake;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
            return ret;
        }
    }
}
