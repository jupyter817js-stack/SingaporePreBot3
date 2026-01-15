using MasterDevs.ChromeDevTools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SingaporePreBot3
{
    public class SiteData
    {
        public string site;
        public List<MatchItem> matches;
        public SiteData(string _site)
        {
            site = _site;
            matches = new List<MatchItem>();
        }
    }
    public class MatchItem
    {
        public string id;
        public bool isOne;              // true - 1X2 add, false - only AH markets
        public bool isChecked = false;  //is compared or not
        public string leagueId;
        public string matchId;
        public DateTime startTime;
        public string leagueName;
        public string home;
        public string away;
        public int homeScore;
        public int awayScore;
        public int minute;
        public int second;
        public bool isRest;
        public int period;
        public DateTime updateTime;
        public List<Market> markets = new List<Market>();       // current Markets

        public List<Market> oldMarkets = new List<Market>();    // old Markets for checking update - start Time
        public List<Market> newMarkets = new List<Market>();    // new Markets for checking update - end Time

        //public List<OverUnderMarket> listOUs = new List<OverUnderMarket>();
        public MatchItem()
        {
            updateTime = DateTime.Now;
        }
        [JsonIgnore]
        public string strTime { get { return $"{startTime.ToString("MM-dd HH:mm")}"; } }
        [JsonIgnore]
        public string strScore { get { return $"{homeScore}-{awayScore}"; } }
        [JsonIgnore]
        public string strMarket { get {
            //List<Market> ret = markets.OrderBy(o => o.nType).OrderBy(o => o.period).ToList();
            //return ret.Count > 0 ? $"{string.Join(", ", ret.Select(o => $"{o.strMarketName}({o.dOdd1}/{o.dOdd2})"))}" : "";
            return markets.Count > 0 ? $"{string.Join(", ", markets.Select(o => $"{o.strMarketName}({o.dOdd1}/{o.dOdd2})"))}" : "";
        }}
        [JsonIgnore]
        public string strOldMarket
        {
            get
            {
                return oldMarkets.Count > 0 ? $"{string.Join(", ", oldMarkets.Select(o => $"{o.strMarketName}({o.dOdd1}/{o.dOdd2})"))}" : "";
            }
        }
        [JsonIgnore]
        public string strNewMarket
        {
            get
            {
                return newMarkets.Count > 0 ? $"{string.Join(", ", newMarkets.Select(o => $"{o.strMarketName}({o.dOdd1}/{o.dOdd2})"))}" : "";
            }
        }
        [JsonIgnore]
        public string strMatchName { get { return $"{home} vs {away}"; } }
        public MatchItem(MatchItem old)
        {
            id = old.id;
            leagueId = old.leagueId;
            matchId = old.matchId;
            leagueName = old.leagueName;
            home = old.home;
            away = old.away;
            homeScore = old.homeScore;
            awayScore = old.awayScore;
            period = old.period;
            minute = old.minute;
            second = old.second;
            updateTime = DateTime.Now;
            startTime = old.startTime;
            markets = new List<Market>();
        }
        public void updateMatch(MatchItem item)
        {
            homeScore = item.homeScore;
            awayScore = item.awayScore;
            minute = item.minute;
            second = item.second;
        }

        public void UpdateMarkets(List<Market> updateMarkets)
        {
            for (int i = 0; i < updateMarkets.Count; i++)
            {
                bool findFlag = false;
                for ( int j = 0; j < markets.Count; j++)
                {
                    if (updateMarkets[i].isSameMarket(markets[j]))
                    {
                        markets[j].nType = updateMarkets[i].nType;
                        markets[j].period = updateMarkets[i].period;
                        markets[j].matchId = updateMarkets[i].matchId;
                        markets[j].marketId = updateMarkets[i].marketId;
                        markets[j].strLine = updateMarkets[i].strLine;
                        markets[j].dOdd1 = updateMarkets[i].dOdd1;
                        markets[j].dOdd2 = updateMarkets[i].dOdd2;
                        markets[j].outcome1 = updateMarkets[i].outcome1;
                        markets[j].outcome2 = updateMarkets[i].outcome2;
                        markets[j].status = updateMarkets[i].status;
                        findFlag = true;
                        break;
                    }
                }
                if (!findFlag)
                    markets.Add(updateMarkets[i]);
            }
        }
    }
}
