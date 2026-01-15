using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingaporePreBot3
{
	public class CalcHelper
	{
		private static double getLevelFromString(string strSiteCode, string strGoalLine)
		{
			double dLevel = 0.0;
			return Utils.ParseToDouble(strGoalLine);
			//if (strSiteCode == "B365")
			//{
			//	string[] strLevels = strGoalLine.Split(',');
			//	for (int i = 0; i < strLevels.Length; i++)
			//	{
			//		double dLevelItem = Convert.ToDouble(strLevels[i]);
			//		dLevelItem = dLevelItem > 0 ? dLevelItem : -dLevelItem;
			//		dLevel += dLevelItem;
			//	}
			//	dLevel /= (double)strLevels.Length;
			//	return dLevel;
			//}
			//else if(strSiteCode == "PB")
			//{
			//	string[] strLevels = strGoalLine.Split('/');
			//	for (int i = 0; i < strLevels.Length; i++)
			//	{
			//		double dLevelItem = Convert.ToDouble(strLevels[i]);
			//		dLevelItem = dLevelItem > 0 ? dLevelItem : -dLevelItem;
			//		dLevel += dLevelItem;
			//	}
			//	dLevel /= (double)strLevels.Length;
			//	return dLevel;
			//}
			//else if(strSiteCode == "SB")
			//{
			//	string[] strLevels = strGoalLine.Split('-');
			//	for (int i = 0; i < strLevels.Length; i++)
			//	{
			//		double dLevelItem = Convert.ToDouble(strLevels[i]);
			//		dLevelItem = dLevelItem > 0 ? dLevelItem : -dLevelItem;
			//		dLevel += dLevelItem;
			//	}
			//	dLevel /= (double)strLevels.Length;
			//	return dLevel;
			//}
			return 0;
		}
		public static bool compareGoalLineString(string strMasterCode, string strMasterGoalLine, string strSlaveCode, string strSlaveGoalLine)
		{
			double dMasterLevel = getLevelFromString(strMasterCode, strMasterGoalLine);
			double dSlaveLevel = getLevelFromString(strSlaveCode, strSlaveGoalLine);
			if (dMasterLevel == dSlaveLevel)
				return true;
			return false;
		}

		public static int calcMinutesFromTimeString(string strTime)
		{
			if (string.IsNullOrEmpty(strTime) || strTime.Length < 3)
				return 0;
			string[] strTimeSplits = strTime.Split(':');
			if (strTimeSplits[0] != null && strTimeSplits[0].Length > 0)
				return Convert.ToInt32(strTimeSplits[0]);
			else
				return 0;
		}
	}
}
