using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SingaporePreBot3
{
	public class BettingController
	{
		private static BettingController _instance = new BettingController();
		private Thread workThread = null;
		private static bool _bAutomation = false;
		DateTime dtLastBetTime = DateTime.Parse("1990-01-01");
		public static bool isStarted
		{
			get
			{
				return _bAutomation;
			}
		}
		public static BettingController Instance
		{
			get
			{
				return _instance;
			}
		}

		public void startAutomation()
		{
            if (workThread != null && workThread.IsAlive)
				return;
			_bAutomation = true;
			Global.WriteStatus("Auto betting started");
			//Setting.Instance.spCtrl.start();
			Setting.Instance.mbCtrl.start();
			//RelationCtrl.Instance.start();
			workThread = new Thread(() => lookupMatch());
			workThread.Start();
		}

		private void stopAutomationInternal()
		{
			_bAutomation = false;
			Global.WriteStatus("Auto betting stop");
		}

		public void stopAutomation()
		{
			try
			{
				stopAutomationInternal();
				//Setting.Instance.spCtrl.stop();
				Setting.Instance.mbCtrl.stop();
				if (workThread != null && workThread.IsAlive)
					workThread.Abort();
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"stopAutomation Error - {ex.Message}");
			}
		}

		private void lookupMatch()
		{
            
            while (BettingController._bAutomation)
			{

                try
                {
                    Thread.Sleep(500);
                    //Check the time interval
                    if (dtLastBetTime.AddSeconds(Setting.Instance.successWaitTime) > DateTime.Now)
						continue;

					PlaceBetCandidate betCandidate = Setting.Instance.getCandidate();
					if (betCandidate == null)
						continue;
					betCandidate = calcStake(betCandidate);
					if (betCandidate.betMaster.dStake == 0)
						continue;
					////if (SettingController.Instance.settings.isBetTest)
					////{
					////	if (betCandidate.strMasterCode != "PB" && betCandidate.strSlaveCode != "PB")
					////		continue;
					////	BetData betData = betCandidate.strMasterCode == "PB" ? betCandidate.betMaster : betCandidate.betSlave;
					////	if (betData.nSide == 0 || betData.strHandicap == "0")
					////		continue;
					////}
					
					if (betCandidate.dProfit < 0 || !betCandidate.hasProfit)
					{
						Global.WriteStatus($"No profitable {betCandidate.strMasterCode}/{betCandidate.strSlaveCode}-{betCandidate.betMaster.eventName}");
						continue;
					}

					//if (!checkBetCondition(betCandidate))
					//	continue;

					if (betCandidate.isAlreadyExist())
					{
						Global.WriteStatus($"Already Exist {betCandidate.strMasterCode}/{betCandidate.strSlaveCode}-{betCandidate.betMaster.eventName}");
						continue;
					}
					if (betCandidate.strSlaveCode == "SP")
					{
						betCandidate.strSlaveCode = betCandidate.strMasterCode;
						betCandidate.strMasterCode = "SP";
						BetData tmpData = betCandidate.betMaster;
						betCandidate.betMaster = betCandidate.betSlave;
						betCandidate.betSlave = tmpData;
					}
					PlaceBetCandidate newCand = new PlaceBetCandidate(betCandidate);
					BetOrder newOrder = new BetOrder(newCand);

                    Global.WriteStatus($"Find new candidation - {newCand.betMaster.eventName}-({newCand.betMaster.strMarketName}/{newCand.strMasterCode}/{newCand.betMaster.dOdds}/{newCand.betMaster.dStake}) v ({newCand.betSlave.strMarketName}/{newCand.strSlaveCode}/{newCand.betSlave.dOdds}) - {newCand.dProfit}%(Arb)");
					
					Trace.WriteLine($"New Candidate - {JsonConvert.SerializeObject(newCand)}");

					if (!checkBalance(newCand))
					{
						Global.WriteStatus("One site has small balance. Please deposit and try again.");
						stopAutomationInternal();
						break;
					}
					var eventForAddSlip = new ManualResetEventSlim();
					AddSlipResult addSlipRes = null;
					ISiteController masterCtrl = getSiteController(newCand.strMasterCode);
					Task.Run(() =>
					{
						try
						{
							addSlipRes = masterCtrl.addSlip(newCand.betMaster);
							processAddSlipResult(newCand.strMasterCode, newCand.betMaster, addSlipRes);
						}
						catch (Exception ex)
						{
                        }
                        eventForAddSlip.Set();
					});
					eventForAddSlip.Wait();

					if (addSlipRes.bSuccess)        //Both Betslip success
					{
						if (checkProfit(Utils.calcProfit(newCand.betMaster.dOdds, newCand.betSlave.dOdds)))     //Profit valid
						{
                            BetResult resMaster = masterCtrl.confirmBet(newCand.betMaster);
                            Trace.WriteLine($"Stake before processBetResult - {newCand.betMaster.dStake}");
                            processBetResult(newCand.strMasterCode, newCand.betMaster, resMaster);
                            if (resMaster.bSuccess)
                            {
                                playMP3(1);
                                newOrder.nOrderStatus = 1;
                                newOrder.betCandidate.updateProfit();
                                Setting.Instance.orderList.Add(newOrder);
                                dtLastBetTime = DateTime.Now;
                                Global.BetFinish();
                            }
                            else
                            {
                                //Global.WriteStatus($"Master site bettting failed");
                                newOrder.nOrderStatus = -3;
                                newOrder.reason = resMaster.strMessage;
                                playMP3(2);
                                Global.BetFinish();
                            }
                        }
						else 
						{
                            Global.WriteStatus($"Odd changed and not profitable. Cancel betting - {newCand.betMaster.eventName} - {newCand.betMaster.strMarketName}({newCand.betMaster.sectionId})/{newCand.betMaster.dOdds}/{newCand.betMaster.dStake}");
                            newOrder.nOrderStatus = -3;
                        }
                    }
					else
					{
						//Global.WriteStatus($"Betting failed - Reason({addSlipRes.strMessage})");
						//if (!addSlipResSlave.bSuccess && newCand.strSlaveCode == "B365")
						//	slaveController.login();
						newOrder.nOrderStatus = -3;
					}
					masterCtrl.removeSlip(newCand.betMaster);

                    if (newOrder.nOrderStatus == -3)
                    {
                        Thread.Sleep(Setting.Instance.successWaitTime * 1000);
                    }
                    //               var eventForAddSlipMaster = new ManualResetEventSlim();
                    //               var eventForAddSlipSlave = new ManualResetEventSlim();
                    //AddSlipResult addSlipResMaster = null;
                    //AddSlipResult addSlipResSlave = null;
                    //ISiteController masterCtrl = getSiteController(newCand.strMasterCode);
                    //ISiteController slaveCtrl = getSiteController(newCand.strSlaveCode);
                    //Task.Run(() =>
                    //               {
                    //                   try
                    //                   {
                    //                       addSlipResMaster = masterCtrl.addSlip(newCand.betMaster);
                    //                       processAddSlipResult(newCand.strMasterCode, newCand.betMaster, addSlipResMaster);
                    //                   }
                    //                   catch (Exception ex)
                    //                   {

                    //                   }
                    //                   eventForAddSlipMaster.Set();
                    //               });
                    //               Task.Run(() =>
                    //               {
                    //                   try
                    //                   {
                    //                       addSlipResSlave = slaveCtrl.addSlip(newCand.betSlave);
                    //                       processAddSlipResult(newCand.strSlaveCode, newCand.betSlave, addSlipResSlave);
                    //                   }
                    //                   catch (Exception ex)
                    //                   {

                    //                   }
                    //                   eventForAddSlipSlave.Set();
                    //               });
                    //               eventForAddSlipMaster.Wait();
                    //               eventForAddSlipSlave.Wait();

                    //if (addSlipResMaster.bSuccess && addSlipResSlave.bSuccess)        //Both Betslip success
                    //{
                    //	newCand.betSlave.dReverseOdds = addSlipResMaster.dOdds;
                    //	newCand.betMaster.dReverseOdds = addSlipResSlave.dOdds;
                    //	//Can place bet, but before done, should be checked if it has profit or not...
                    //	newCand = calcStake(newCand);
                    //	if (checkProfit(CalcHelper.calcProfit(newCand.betMaster.dOdds, newCand.betSlave.dOdds)))     //Profit valid
                    //	{
                    //		BetResult resMaster = masterCtrl.confirmBet(newCand.betMaster);
                    //		processBetResult(newCand.strMasterCode, newCand.betMaster, resMaster);
                    //		if (resMaster.bSuccess)
                    //		{
                    //			newCand.betSlave.dReverseOdds	= newCand.betMaster.dOdds;
                    //			//Global.WriteStatus($"Slave site bettting on {newCand.strMasterCode}");
                    //			BetResult resSlave = slaveCtrl.confirmBet(newCand.betSlave);
                    //			processBetResult(newCand.strSlaveCode, newCand.betSlave, resSlave);
                    //			if (resSlave.bSuccess == true)
                    //			{
                    //				bool profitStatus = checkProfit(CalcHelper.calcProfit(newCand.betMaster.dOdds, newCand.betSlave.dOdds));
                    //				newOrder.nOrderStatus = profitStatus ? 1 : -1;
                    //				playMP3(1); 
                    //				dtLastBetTime = DateTime.Now;
                    //				Global.BetFinish();
                    //			}
                    //			else            ///Slave bet falied case
                    //			{
                    //				newOrder.nOrderStatus = -2;
                    //				newOrder.reason = resSlave.getStrCode();
                    //				playMP3(2);
                    //				Global.BetFinish();
                    //				break;
                    //				////Global.WriteStatus("Slave bettting failed auto supplying");
                    //				//if (!tryAutoSupplement(newCand))
                    //				//{

                    //				//}
                    //				//else
                    //				//{
                    //				//	Global.WriteStatus($"自动补单成功了。");
                    //				//	newOrder.nOrderStatus = -1;
                    //				//	LogController.Instance.BetFinished();
                    //				//}
                    //			}
                    //		}
                    //		else
                    //		{
                    //			//Global.WriteStatus($"Master site bettting failed");
                    //			newOrder.nOrderStatus = -3;
                    //		}

                    //	}
                    //	else
                    //	{
                    //		Global.WriteStatus($"Odd changed and not profitable. Cancel betting");
                    //		newOrder.nOrderStatus = -3;
                    //	}
                    //}
                    //else
                    //{
                    //	Global.WriteStatus("One side betting failed or canceled finding next candidate");
                    //	//if (!addSlipResSlave.bSuccess && newCand.strSlaveCode == "B365")
                    //	//	slaveController.login();
                    //	newOrder.nOrderStatus = -3;
                    //}
                    //masterCtrl.removeSlip(newCand.betMaster);
                    //slaveCtrl.removeSlip(newCand.betSlave);
                    ////Global.WriteStatus($"Master site bettting on {newCand.strMasterCode}");

                    //if (newOrder.nOrderStatus == -3)
                    //{
                    //	newCand.tryCount++;
                    //	Thread.Sleep(Setting.Instance.successWaitTime * 1000);
                    //}
                }
				catch (Exception ex)
				{
					string error = ex.Message;
				}
			}
		}

		public void playMP3(int type = 1)
		{
			if (!Setting.Instance.playSound)
				return;
			Task.Run(() =>
			{
				try
				{
					if (type == 1)
					{
						SoundPlayer player = new SoundPlayer(Properties.Resources.success);
						player.Play();
					}
					else if (type == 2)
					{
						SoundPlayer player = new SoundPlayer(Properties.Resources.failed);
						player.Play();
					}
				}
				catch (Exception ex)
				{
					string error = ex.Message;
				}
			});
		}

		public void processAddSlipResult(string siteCode, BetData data, AddSlipResult ret)
		{
			try
			{
				string siteName = getSiteName(siteCode);
				if (ret.bSuccess)
					Global.WriteStatus($"[{siteName}] Betslip success - {data.eventName} - {data.strMarketName}({data.sectionId})/{data.dOdds}/{data.dStake}");
				else
					Global.WriteStatus($"[{siteName}] Betslip failed - {data.eventName} - {data.strMarketName}({data.sectionId})/{data.dOdds}/{data.dStake} Reason - {getReason(ret.strMessage)}");
			}
			catch (Exception ex)
			{
				string error = ex.Message;
			}
		}
		public void processBetResult(string siteCode, BetData data, BetResult ret)
		{
			try
			{
				string siteName = getSiteName(siteCode);
				if (ret.bSuccess)
					Global.WriteStatus($"[{siteName}] Betting successed - {data.eventName} - {data.strMarketName}({data.sectionId})/{data.dOdds}/{data.dStake}");
				else
					Global.WriteStatus($"[{siteName}] Betting failed - {data.eventName} - {data.strMarketName}({data.sectionId})/{data.dOdds}/{data.dStake} Reason - {getReason(ret.strMessage)}");
			}
			catch (Exception ex)
			{
				string error = ex.Message;
			}
		}
		public string getSiteName(string code)
		{
			string name = string.Empty;
			try
			{
				switch (code)
				{
					case "SP":
						name = "Singaporepool";
						break;
					case "MB":
						name = "Mollybet";
						break;
					default:
						name = code;
						break;
				}
			}
			catch (Exception ex)
			{
				string error = ex.Message;
			}
			return name;
		}
		private string getReason(string old)
		{
			string ret = old;
			try
			{
				if (string.IsNullOrEmpty(ret) || ret == "Bet override found")
					return "Market Suspended";
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
			}
			return ret;
		}
		private bool checkBalance(PlaceBetCandidate betCandidate)
		{
			return true;
		}
		//Calculate the real betting stake for the sites
		private PlaceBetCandidate calcStake(PlaceBetCandidate betCandidate)
		{
			//Master Stake 
			betCandidate.updateProfit();
			betCandidate.betMaster.dStake = Setting.Instance.getStake(betCandidate);
			//Slave stake
			double dMasterProfit = betCandidate.betMaster.dOdds * betCandidate.betMaster.dStake;
			double dSlaveStake = dMasterProfit / betCandidate.betSlave.dOdds;
			betCandidate.betSlave.dStake = Utils.convertStake(dSlaveStake);
			if (dSlaveStake + betCandidate.betMaster.dStake < dMasterProfit)
			{
				//will get profit				
				betCandidate.hasProfit = true;
			}
			else
			{
				//will not get profit
				betCandidate.hasProfit = false;
			}
			return betCandidate;
		}

		private void sortCandidate(ref List<PlaceBetCandidate> listCandidate)
		{
			//Sort candidates by rules
			listCandidate.Sort((x, y) => { return x.dProfit > y.dProfit ? 1 : -1; });
		}
		private bool checkProfit(double profit)  //1: Normal betting		2: Auto supply
		{
			try
			{
				double dProfit = profit * 100;
				if (dProfit >= Setting.Instance.percentMin && dProfit <= Setting.Instance.percentMax)
					return true;
			}
			catch (Exception ex)
			{
				string error = ex.Message;
			}
			return false;
		}
		public int checkOdd(double odd, double reverseOdd)
		{
			int ret = -1;
			try
			{
				if (odd < 1)
					return -1;
				if (reverseOdd == 0)
					return 1;
				double profit = Utils.calcProfit(odd, reverseOdd);
				if (checkProfit(profit))
					ret = 1;
			}
			catch (Exception ex) 
			{
                Trace.WriteLine($"CheckingOdd Error - {ex.Message}");
            }
            return ret;
		}
		private ISiteController getSiteController(string strSiteCode)
		{
			if (strSiteCode == "SP")
				return Setting.Instance.spCtrl;
			else if (strSiteCode == "MB")
				return Setting.Instance.mbCtrl;
			else
				return null;
		}
	}
}
