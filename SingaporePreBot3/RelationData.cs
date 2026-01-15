using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SingaporePreBot3
{
	public class RelationItem
	{
		public string site1;
		public string site2;
		public List<BindItem> bindList = new List<BindItem>();
		public RelationItem(string _site1, string _site2)
		{
			site1 = _site1;
			site2 = _site2;
		}
	}
	public class BindItem
	{
		public string matchId1;
		public string matchId2;
		public DateTime createTime;
		public BindItem(string id1, string id2)
		{
			matchId1 = id1;
			matchId2 = id2;
			createTime = DateTime.Now;
		}
	}
	public class BetData
	{
		public double dStake;
		public double dOdds;
		public int period;			// 0- Full Half 1- Half Time
		public int nType;           // 0- Goal Handicap 1- Goal OU 2- Corner Handicap 3- Corner OU
		public string strHandicap;  // Handicap or GoalLine Value like : -0.5
		public int nSide;           // 0- Home or Over, 1-Away or Under
		public string eventId;
		public string bettingId;
		public string sectionId;
		public string siteCode;
		public string eventName;
		public double dReverseOdds;
		public bool isManual;
		public int status;          // 0- Init		1- Place bet	2- Accept	3- Reject	4- Canceled
		public string slipContent;
		public string strMarketName { get {
				string type = string.Empty;
				if (nType == 0)
				{
					double line = Utils.ParseToDouble(strHandicap);
					if (nSide == 0)
						type = $"AH1({line})";
					else
						type = $"AH2({-1 * line})";
				}
				else if (nType == 1)
				{
					type = nSide == 0 ? $"Over({strHandicap})" : $"Under({strHandicap})";
				}
				return $"{(period == 0 ? "FT" : "HT")}-{type}";
		}}
		public BetData() { }
		public BetData(BetData baseItem) 
		{
			dStake = baseItem.dStake;
			dOdds = baseItem.dOdds;
			period = baseItem.period;
			nType = baseItem.nType;
			strHandicap = baseItem.strHandicap;
			nSide = baseItem.nSide;
			eventId = baseItem.eventId;
			bettingId = baseItem.bettingId;
			sectionId = baseItem.sectionId;
			siteCode = baseItem.siteCode;
			eventName = baseItem.eventName;
			dReverseOdds = baseItem.dReverseOdds;
			isManual = baseItem.isManual;
			status = baseItem.status;
			slipContent = baseItem.slipContent;
		}
		public bool isSameBet(BetData data)
		{
			return eventId == data.eventId && period == data.period && nType == data.nType && strHandicap == data.strHandicap && nSide == data.nSide;
		}
		public double getLine()
		{
			if (nType % 2 == 1)
				return Utils.ParseToDouble(strHandicap);
			return nSide == 0 ? Utils.ParseToDouble(strHandicap) : -1 * Utils.ParseToDouble(strHandicap);
		}
		public BetData(Market market, string matchName, string matchId, string strSite, int side)
		{
			dOdds = side == 0 ? market.dOdd1 : market.dOdd2;
			period = market.period;
			nType = market.nType;
			nSide = side;
			strHandicap = market.strLine; //Always keep Home Line //side == 0 ? market.strLine : market.getOtherLine();
			eventId = matchId;
			siteCode = strSite;
			eventName = matchName;
			if (siteCode == "SP")
			{
				sectionId = side == 0 ? market.outcome1 : market.outcome2;
			}
			else
			{
				sectionId = market.marketId;
			}
		}
	}
	public class PlaceBetCandidate
	{
		public int index;
		public string league;
		public string home;
		public string away;
		public bool hasProfit;
		public double dProfit;
		public string strMasterCode;
		public string strSlaveCode;
		public BetData betMaster;
		public BetData betSlave;
		public bool masterPlaced;
		public bool slavePlaced;
		public DateTime masterPlaceTime;
		public DateTime slavePlaceTime;
		public string masterPlaceId;
		public string slavePlaceId;
		public string strTime;
		public string strScore;
		public bool isOneSide;
		public string oneSideCode;
		public DateTime createTime = DateTime.Now;
		public int getExistCount()
		{
			int ret = 0;
			try
			{
				ret = Setting.Instance.orderList.Where(o => o.nOrderStatus != -3 && o.isSameCandidate(this)).Count();
            }
			catch (Exception ex)
			{
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
			return ret;
		}
		public bool isAlreadyExist()
		{
			int existCount = getExistCount();
            if (existCount == 0)
				return false;
			if (existCount == 1)
			{
				if (dProfit > 3)
					return false;
				else
					return true;
			}
			if (existCount == 2)
			{
				if (dProfit > 5)
					return false;
				else
					return true;
			}
			if (existCount == 3)
			{
				if (dProfit > 7)
					return false;
				else
					return true;
			}
			if (existCount == 4)
			{
				if (dProfit > 9)
					return false;
				else
					return true;
			}
			if (existCount == 5)
			{
				if (dProfit > 11)
					return false;
				else
					return true;
			}
			return true;
		}
		public void updateProfit()
		{
			dProfit = Math.Round(100 * Utils.calcProfit(betMaster.dOdds, betSlave.dOdds), 2);
		}
		public bool checkCandidate()
		{
			//if (strMasterCode == "B365" || (setItem.bOverFirst && strSlaveCode != "B365" && betMaster.nType % 2 == 1 && betMaster.nSide == 1) || (strSlaveCode != "B365" && (setItem.bUpperFirst && Utils.ParseToDouble(betMaster.strHandicap) < 0 || !setItem.bUpperFirst && Utils.ParseToDouble(betMaster.strHandicap) > 0)))     //Master site == B365 or Master bet is Goal Under Swap master and slave
			//if (setItem.bHDPOrder && betMaster.nType % 2 == 0 && (setItem.bPlusFirst && Utils.ParseToDouble(betMaster.strHandicap) < 0 || !setItem.bPlusFirst && Utils.ParseToDouble(betMaster.strHandicap) > 0) ||
			//	setItem.bOUOrder && betMaster.nType % 2 == 1 && (setItem.bOverFirst && betMaster.nSide == 1 || !setItem.bOverFirst && betMaster.nSide == 0))
			//{
			//	string tmp = strMasterCode;
			//	BetData tmpBet = betMaster;
			//	strMasterCode = strSlaveCode;
			//	betMaster = betSlave;
			//	strSlaveCode = tmp;
			//	betSlave = tmpBet;
			//}
			
			if (Setting.Instance.percentMin > dProfit || dProfit > Setting.Instance.percentMax)                                          //Check Profit
				return false;

			int masterBetCount = Setting.Instance.orderList.Where(o => o.nOrderStatus != -3 && (betMaster.eventId == o.betCandidate.betMaster.eventId || betMaster.eventId == o.betCandidate.betSlave.eventId)).Count();
			int slaveBetCount = Setting.Instance.orderList.Where(o => o.nOrderStatus != -3 && (betSlave.eventId == o.betCandidate.betMaster.eventId || betSlave.eventId == o.betCandidate.betSlave.eventId)).Count();
			
			return true;
		}
		public PlaceBetCandidate() { }
		public PlaceBetCandidate(PlaceBetCandidate baseItem) 
		{
			index = baseItem.index;
			league = baseItem.league;
			home = baseItem.home;
			away = baseItem.away;
			hasProfit = baseItem.hasProfit;
			dProfit = baseItem.dProfit;
			strMasterCode = baseItem.strMasterCode;
			strSlaveCode = baseItem.strSlaveCode;
			betMaster = new BetData(baseItem.betMaster);
			betSlave = new BetData(baseItem.betSlave);
			masterPlaced = baseItem.masterPlaced;
			slavePlaced = baseItem.slavePlaced;
			masterPlaceTime = baseItem.masterPlaceTime;
			slavePlaceTime = baseItem.slavePlaceTime;
			masterPlaceId = baseItem.masterPlaceId;
			slavePlaceId = baseItem.slavePlaceId;
			strTime = baseItem.strTime;
			strScore = baseItem.strScore;
			isOneSide = baseItem.isOneSide;
			oneSideCode = baseItem.oneSideCode;
			createTime = DateTime.Now;
		}
	}
	public class BetResult
	{
		public bool bSuccess;
		public string strMessage;
		public int code;			//1:success	 2:Supply	-1:login Error	-2:Market is not available  -3:Betslip failed -4:Odd changed -5:Not profitable -6:Betting failed  -7:Market changed
		public string bettingId;
		public double balance;
		public double odd;
		public string getStrCode()
		{
			string ret = string.Empty;
			switch (code)
			{
				case 1:
					ret = "Success";
					break;
				case 2:
					ret = "Supply";
					break;
				case -1:
					ret = "Login failed";
					break;
				case -2:
					ret = "Market is not avaliable";
					break;
				case -3:
					ret = "Betslip failed";
					break;
				case -4:
					ret = "Odd changed";
					break;
				case -5:
					ret = "Not profitable";
					break;
				case -6:
					ret = "Betting failed";
					break;
				case -7:
					ret = "Market changed";
					break;
				default:
					ret = "";
					break;
			}
			return ret;
		}
	}
	public class BetOrder
	{
		//Display Data
		public string strMasterSite;
		public string strSlaveSite;
		public string strLeague;
		public string strHome;
		public string strAway;
		public string strGameTime;
		public string strGoal;
		public string strMasterBetType;
		public string strSlaveBetType;
        public string masterOdd { get { return betCandidate != null ? betCandidate.betMaster.dOdds.ToString() : ""; } }
		public string slaveOdd { get { return betCandidate != null ? betCandidate.betSlave.dOdds.ToString() : ""; } }
		public string masterStake { get { return betCandidate != null ? betCandidate.betMaster.dStake.ToString() : ""; } }
		public string slaveStake { get { return betCandidate != null ? betCandidate.betSlave.dStake.ToString() : ""; } }
		public double dProfit { get { return betCandidate.dProfit; } }
		public string masterMatchId;
		public string masterOddId;
		public string slaveMatchId;
		public string slaveOddId;
		public string reason;
		public string getStatus()
		{
			string ret = string.Empty;
			switch (nOrderStatus)
			{
				case 1:
					ret = betCandidate.isOneSide ? BettingController.Instance.getSiteName(betCandidate.oneSideCode) + "-One site success" : "Success";
					break;
				case 2:
					ret = "Auto Supplyed";
					break;
				case -1:
					ret = "Auto Supply";
					break;
				case -2:
					ret = "Manual Supply";
					break;
				case -3:
					ret = "First bet failed";
					break;
				case -4:
					string cancelSiteCode = betCandidate.betMaster.status == 3 ? strMasterSite : strSlaveSite;
					ret = $"{BettingController.Instance.getSiteName(cancelSiteCode)}-Betting canceled";
					break;
				default:
					ret = "Processing";
					break;
			}
			return ret;
		}
		public DateTime orderTime;

		//Internal Data
		public int nOrderIndex;
		public int nOrderStatus; // 1- success, 2-补单   -1 - auto complement -2 - need manual process, -3 - failed 1st , -4 - Bet Rejected
		public PlaceBetCandidate betCandidate;
		public BetOrder() { }
		public BetOrder(PlaceBetCandidate item)
		{
			betCandidate = item;
			strMasterSite = item.strMasterCode;
			strSlaveSite = item.strSlaveCode;
			strLeague = item.league;
			strGoal = item.strScore;
			strGameTime = item.strTime;
			strHome = item.home;
			strAway = item.away;
			//string marketType = string.Empty;
			//if (item.betMaster.nType % 2 == 0)		///Handicap
			//{
			//	double line = item.betMaster.nSide == 0 ? Utils.ParseToDouble(item.betMaster.strHandicap) : Utils.ParseToDouble(item.betMaster.strHandicap) * -1;
			//	if (line > 0)
			//		marketType = $"+{line}";
			//	else if (line < 0)
			//		marketType = $"-{line * -1}";
			//	else
			//		marketType = $"0";
			//}
			//else				//Over / Under
			//{
			//	marketType = $"{(item.betMaster.nSide == 0 ? "大于" : "小于")} {item.betMaster.strHandicap}";
			//}
			//strBetType = $"{(item.betMaster.nTimeType == 0 ? "全场" : "半场")}-{(item.betMaster.nType % 2 == 0 ? "让球" : "大小盘")}{(item.betMaster.nType > 1 ? "(角球)" : "")}- {marketType}";
			strMasterBetType = item.betMaster.strMarketName;
            strSlaveBetType = item.betSlave.strMarketName;
			masterMatchId = item.betMaster.eventId;
			masterOddId = item.betMaster.sectionId;
			slaveMatchId = item.betSlave.eventId;
			slaveOddId = item.betSlave.sectionId;
			orderTime = DateTime.Now;
		}
		public bool isSameCandidate(PlaceBetCandidate cand)
		{
			if (betCandidate.strMasterCode == cand.strMasterCode)
			{
				return cand.betMaster.isSameBet(betCandidate.betMaster) && cand.betSlave.isSameBet(betCandidate.betSlave);
			}
			else
			{
				return cand.betMaster.isSameBet(betCandidate.betSlave) && cand.betSlave.isSameBet(betCandidate.betMaster);
			}
		}
	}
	public class AddSlipResult
	{
		public bool bSuccess;
		public double dOdds;
		public string strMessage;
		public int code;
	}
	public class BetslipData
	{
		public double dLine;
		public int nType;
		public int nTimeType;
		public double dOdd;
		public string strMarket;
	}
	public class StakeItem
	{
		public double min;
        public double max;
        public double stake;
		public int count;
		public StakeItem() { }
		public StakeItem(double _min, double _max, double _stake, int _count)
		{
			min = _min;
			max = _max;
			stake = _stake;
			count = _count;
		}
	}
}
