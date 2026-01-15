
namespace SingaporePreBot3
{
	public class Market
	{
		public int nType;           // 0- Asian Handicap 1- Goal OU 2- Head to Head 
		public int period;          // 0- Match Other-Period number
		public string matchId;
		public string marketId;
		public string strLine;
		public double dOdd1;
		public double dOdd2;
		public string outcome1;
		public string outcome2;
		public string status;
		public long stamp;
		public bool updated;
		public string strMarketName
		{
			get 
			{
                string strType = nType == 0 ? $"AH({strLine})" : (nType == 1 ? $"OU({strLine})" : $"1X2({strLine})");
				return $"{(period == 0 ? "FT" : "HT")}-{strType}";
			}
		}
		public Market() { }
		public Market(int _nType, int _period, string _matchId, string _marketId, string _strLine, double _dOdd1, double _dOdd2, string _outcome1, string _outcome2, string _status)
		{
			nType = _nType;
			period = _period;
			matchId = _matchId;
			marketId = _marketId;
			strLine = _strLine;
			dOdd1 = _dOdd1;
			dOdd2 = _dOdd2;
			outcome1 = _outcome1;
			outcome2 = _outcome2;
			status = _status;
		}
		public Market(Market old)
		{
			nType = old.nType;
			period = old.period;
			matchId = old.matchId;
			marketId = old.marketId;
			strLine = old.strLine;
			dOdd1 = old.dOdd1;
			dOdd2 = old.dOdd2;
			outcome1 = old.outcome1;
			outcome2 = old.outcome2;
			status = old.status;
		}
		public bool isSameMarket(Market other)		///Can't use for SB
		{
			//if (other.marketId == marketId)
			//	return true;
			if (other.nType == nType && other.period == period && other.strLine == strLine)
				return true;
			return false;
		}
		public string getOtherLine()
		{
			string ret = string.Empty;
			if (nType % 2 == 1 || strLine == "0")
				return strLine;
			ret = $"{Utils.ParseToDouble(strLine) * -1}";
			return ret;
		}
	}
	//public class HDPMarket : Market
	//{
	//	public string strHomeHandicap;
	//	public string strAwayHandicap;
	//	public double dHomeOdds;
	//	public double dAwayOdds;
	//}
	//public class OverUnderMarket : Market
	//{
	//	public string strGoalLine;
	//	public double dOverOdds;
	//	public double dUnderOdds;
	//}
}
