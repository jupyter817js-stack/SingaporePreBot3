using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace SingaporePreBot3
{
    public class RelationCtrl
    {
		private static RelationCtrl _instance = new RelationCtrl();
        private List<RelationItem> _relations = new List<RelationItem>();
        Dictionary<string, List<MatchItem>> dataDic = new Dictionary<string, List<MatchItem>>();
        public List<string> _sites = new List<string>();
        Dictionary<string, Mutex> mutexDic = new Dictionary<string, Mutex>();
        Mutex candMutex;
        bool isWorking = false;
        Thread mainThread;
        //public CandidateEvent onCandidate;
        List<PlaceBetCandidate> candidates = new List<PlaceBetCandidate>();
        public int candIndex = 0;
        string tipDir = $"{Application.StartupPath}\\tip";
        public static RelationCtrl Instance
		{
            get
            {
                if (_instance == null)
                {
                    _instance = new RelationCtrl();
                }

                return _instance;
            }
        }

        private RelationCtrl()
        {
            _sites = new List<string>() { "SP", "MB" };
            foreach (string site in _sites)
            {
                mutexDic.Add(site, new Mutex());
                dataDic.Add(site, new List<MatchItem>());
            }
            candMutex = new Mutex();
            _relations.Add(new RelationItem("SP", "MB"));
        }
        public List<MatchItem> getMatches(string site)
        {
            List<MatchItem> ret = null;
            if (!_sites.Contains(site))
                return ret;
            ret = dataDic[site];
            // curSite = _datas.Find(o => o.site == site);
            //if (curSite != null)
            //    return curSite.matches;
            return ret;
        }
        public void setMatches(string site, List<MatchItem> matches)
        {
            if (!_sites.Contains(site))
                return;
            Mutex curMutex = mutexDic[site];
            try
            {
                curMutex.WaitOne();
                dataDic[site] = matches;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} erorr - {ex.Message}");
            }
            finally
            {
                curMutex.ReleaseMutex();
            }
        }

        public void UpdateMatches(string site, List<MatchItem> newMatches)
        {
            if (!_sites.Contains(site))
                return;

            Mutex curMutex = mutexDic[site];
            try
            {
                curMutex.WaitOne();

                if (dataDic.ContainsKey(site))
                {
                    // 기존 리스트 업데이트
                    var oldMatches = dataDic[site];

                    // matchId 기준으로 병합 (중복 처리)
                    foreach (var newItem in newMatches)
                    {
                        var existing = oldMatches.FirstOrDefault(m => m.matchId == newItem.matchId);
                        if (existing != null)
                        {
                            // 속성 갱신
                            existing.startTime = newItem.startTime;
                            existing.home = newItem.home;
                            existing.away = newItem.away;
                            existing.homeScore = newItem.homeScore;
                            existing.awayScore = newItem.awayScore;
                            existing.minute = newItem.minute;
                            existing.second = newItem.second;
                            existing.isRest = newItem.isRest;
                            existing.period = newItem.period;
                            existing.updateTime = DateTime.UtcNow;
                            existing.markets = newItem.markets;
                        }
                        else
                        {
                            oldMatches.Add(newItem);
                        }
                    }
                    oldMatches.RemoveAll(o => !newMatches.Any(n => n.matchId == o.matchId));
                }
                else
                {
                    dataDic[site] = newMatches;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
            finally
            {
                curMutex.ReleaseMutex();
            }
        }
        public void updateSabaTime(string code)
        {
            Mutex curMutex = mutexDic[code];
            try
            {
                curMutex.WaitOne();
                List<MatchItem> matches = dataDic[code];
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"removeAllMatches error - {ex.Message}");
            }
            finally
            {
                curMutex.ReleaseMutex();
            }
        }
        public List<BindItem> getBinding(string site1, string site2)
        {
            List<BindItem> ret = new List<BindItem>();
            RelationItem curRelation = _relations.Find(o => o.site1 == site1 && o.site2 == site2);
            if (curRelation != null)
                return curRelation.bindList;
            return ret;
        }
        public bool isMatched(string site, string matchId)
        {
            bool ret = false;
            try
            {
                ret = _relations.Find(o => o.site1 == site && o.bindList.Count > 0 && o.bindList.Find(p => p.matchId1 == matchId) != null) != null ||
                    _relations.Find(o => o.site2 == site && o.bindList.Count > 0 && o.bindList.Find(p => p.matchId2 == matchId) != null) != null;
            }
            catch { }
            return ret;
        }
        public void start()
        {
            try
            {
                if (mainThread == null || !mainThread.IsAlive)
                {
                    isWorking = true;
                    Global.WriteStatus("Relation Thread Started.");
                    mainThread = new Thread(() => relate());
                    mainThread.Start();
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }
        public void stop()
        {
            try
            {
                isWorking = false;
                if (mainThread != null)
                {
                    Global.WriteStatus("Relation thread Stopped.");
                    mainThread.Abort();
                }
                if (mainThread != null)
                {
                    mainThread.Abort();
                }
            }
            catch (Exception ex)
            {
                Global.WriteStatus(ex.ToString());
            }
        }
        private void relate()
        {
            while (isWorking)
            {
                try
                {
                    for (int i = 0; i < _sites.Count; i++)
                    {
                        for (int j = i + 1; j < _sites.Count; j++)
                        {
                            checkRelation(_sites[i], _sites[j]);
                        }
                        if (_sites[i] != "MB")
                            removeOldMatch(_sites[i]);
                    }
                    List<PlaceBetCandidate> candidates = getCandidate();
                    Setting.Instance.candidateList = candidates;
                    //await socketCtrl.SendData("candidate", candidates);

                    if (candidates.Count > 0)
                    {
                        Trace.WriteLine($"Send {candidates.Count} candidates - {JsonConvert.SerializeObject(candidates)}");
                    }
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"relate error - {ex.Message}");
                }
            }
            Global.UpdateArb();
        }
        public void removeAllMatches(string site)
        {
            if (!_sites.Contains(site))
                return;
            Mutex curMutex = mutexDic[site];
            try
            {
                curMutex.WaitOne();
                dataDic[site] = new List<MatchItem>();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"removeAllMatches error - {ex.Message}");
            }
            finally 
            {
                curMutex.ReleaseMutex();
            }
        }
        private void removeOldMatch(string site)
        {
            if (!_sites.Contains(site))
                return;
            Mutex curMutex = mutexDic[site];
            try
            {
                curMutex.WaitOne();
                List<MatchItem> matches = dataDic[site];
                List<MatchItem> oldMatches = matches.Where(o => o.updateTime < DateTime.Now.AddMinutes(-1)).ToList();
                if (oldMatches.Count > 0)
                    dataDic[site] = matches.Except(oldMatches).ToList();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"removeOldMatch error - {ex.Message}");
            }
            finally
            {
                curMutex.ReleaseMutex();
            }
        }
        public void updateMatch(string site, MatchItem newMatch)
        {
            if (!_sites.Contains(site))
                return;
            Mutex curMutex = mutexDic[site];
            try
            {
                curMutex.WaitOne();
                List<MatchItem> matches = dataDic[site];
                long startTick = Utils.getTick();
                MatchItem oldMatch = matches.Find(o => o.matchId == newMatch.matchId);
                if (oldMatch == null)               ///New Match
                {
                    oldMatch = new MatchItem(newMatch);
                    oldMatch.markets = newMatch.markets.Select(o => { o.stamp = Utils.getTick(); o.updated = true; return o; }).ToList();
                    matches.Add(oldMatch);
                }
                else                                ///Exist Match
                {
                    oldMatch.updateMatch(newMatch);
                    oldMatch.markets = oldMatch.markets.Select(o => { o.updated = false; return o; }).ToList();
                    foreach (Market one in newMatch.markets)
                    {
                        Market findMarket = oldMatch.markets.Find(o => o.isSameMarket(one)); //Find(o => o.nType == one.nType && o.nTimeType == one.nTimeType && o.strLine == one.strLine);
                        if (findMarket != null)
                        {
                            findMarket.stamp = Utils.getTick();
                            if (findMarket.marketId != one.marketId || findMarket.dOdd1 != one.dOdd1 || findMarket.dOdd2 != one.dOdd2)
                            {
                                findMarket.marketId = one.marketId;
                                findMarket.dOdd1 = one.dOdd1;
                                findMarket.dOdd2 = one.dOdd2;
                                findMarket.updated = true;
                            }
                        }
                        else
                        {
                            one.stamp = Utils.getTick();
                            one.updated = true;
                            oldMatch.markets.Add(one);
                        }
                    }
                }
                if (site != "MB")
                {
                    List<Market> removeMarket = oldMatch.markets.Where(o => o.stamp < startTick).ToList();
                    if (removeMarket != null && removeMarket.Count > 0)
                        oldMatch.markets = oldMatch.markets.Except(removeMarket).ToList();
                }
                //List<Market> updateMarket = oldMatch.markets.Where(o => o.updated).ToList();
                if (oldMatch.markets.Count > 0)         //updateMarket != null && updateMarket.Count > 0
                {
                    foreach (Market one in oldMatch.markets)
                    {
                        checkCandidate(site, one);
                    }
                }
                oldMatch.updateTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"udpateMatch erorr - {ex.Message}");
            }
            finally 
            {
                curMutex.ReleaseMutex();
            }
        }
        public void checkSiteCandidate(string site)
        {
            if (!_sites.Contains(site))
                return;
            try
            {
                List<MatchItem> matches = dataDic[site];
                long startTick = Utils.getTick();
                if (matches == null || matches.Count == 0)
                    return;
                foreach (MatchItem match in matches)
                {
                    if (match.markets.Count > 0)         //updateMarket != null && updateMarket.Count > 0
                    {
                        foreach (Market one in match.markets)
                        {
                            one.stamp = Utils.getTick();
                            one.updated = true;
                            checkCandidate(site, one);
                        }
                    }
                    match.updateTime = DateTime.Now;
                }
                //List<Market> updateMarket = oldMatch.markets.Where(o => o.updated).ToList();
                
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"udpateMatch erorr - {ex.Message}");
            }
        }
        public void doCompare(string site)
        {
            List<MatchItem> matches = getMatches(site);
            Setting.Instance.compareResult.Clear();
            // Setting.Instance.comparedMatchId.Clear(); // ❌ 지우면 안 됨

            try
            {
                foreach (MatchItem one in matches)
                {
                    // 이미 알려준 matchId는 건너뛰기
                    if (Setting.Instance.comparedMatchId.Contains(one.matchId))
                        continue;

                    if ((one.oldMarkets.Count != 0) && (one.newMarkets.Count != 0))
                    {
                        List<string> results = FindOneLevelMoves(one.oldMarkets, one.newMarkets, one.isOne, 0.05);
                        if (results.Count > 0)
                        {
                            Setting.Instance.compareResult.Add(
                                $"[{one.leagueName}] {one.home} vs {one.away} ({one.strScore})"
                            );
                            Setting.Instance.compareResult.AddRange(results);

                            // 알림한 경기 ID 저장 → 다음번 호출 시 제외됨
                            Setting.Instance.comparedMatchId.Add(one.matchId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"doCompare error - {ex.Message}");
            }
        }
        public static List<string> FindOneLevelMoves(List<Market> oldMarkets, List<Market> newMarkets, bool isOne, double oddTolerance = 0.05)
        {
            var results = new List<string>();

            // 특수 케이스: 마켓이 2개뿐인 경우
            if (isOne)
            {
                var oldAH = oldMarkets.FirstOrDefault(m => m.nType == 1);  // 예: AH
                var old1X2 = oldMarkets.FirstOrDefault(m => m.nType == 2); // 예: 1X2

                var newAH = newMarkets.FirstOrDefault(m => m.nType == 1);
                var new1X2 = newMarkets.FirstOrDefault(m => m.nType == 2);

                if (oldAH != null && old1X2 != null && newAH != null && new1X2 != null)
                {
                    // odd 비교
                    bool odd1Close = Math.Abs(new1X2.dOdd1 - oldAH.dOdd1) <= oddTolerance;
                    bool odd2Close = Math.Abs(new1X2.dOdd2 - oldAH.dOdd2) <= oddTolerance;

                    if (odd1Close && odd2Close)
                        results.Add($"[Both] Old FT-AH({oldAH.strLine})({oldAH.dOdd1}/{oldAH.dOdd2}) -> New FT-1X2({new1X2.strLine})({new1X2.dOdd1}/{new1X2.dOdd2})");
                    else if (odd1Close)
                        results.Add($"[Odd1] Old FT-AH({oldAH.strLine})({oldAH.dOdd1}) -> New FT-1X2({new1X2.strLine})({new1X2.dOdd1})");
                    else if (odd2Close)
                        results.Add($"[Odd2] Old FT-AH({oldAH.strLine})({oldAH.dOdd2}) -> New FT-1X2({new1X2.strLine})({new1X2.dOdd2})");

                    return results;
                }
            }

            // 일반 케이스: 핸디캡 ±0.25 비교
            foreach (var newM in newMarkets)
            {
                if (!double.TryParse(newM.strLine, out double newHandicap))
                    continue;

                foreach (var oldM in oldMarkets)
                {
                    if (!double.TryParse(oldM.strLine, out double oldHandicap))
                        continue;

                    if (Math.Abs(newHandicap - oldHandicap) == 0.25)
                    {
                        bool odd1Close = Math.Abs(newM.dOdd1 - oldM.dOdd1) <= oddTolerance;
                        bool odd2Close = Math.Abs(newM.dOdd2 - oldM.dOdd2) <= oddTolerance;

                        if (odd1Close && odd2Close)
                            results.Add($"[Both] Old FT-AH({oldM.strLine})({oldM.dOdd1}/{oldM.dOdd2}) -> New FT-AH({newM.strLine})({newM.dOdd1}/{newM.dOdd2})");
                        else if (odd1Close)
                            results.Add($"[Odd1] Old FT-AH({oldM.strLine})({oldM.dOdd1}) -> New FT-AH({newM.strLine})({newM.dOdd1})");
                        else if (odd2Close)
                            results.Add($"[Odd2] Old FT-AH({oldM.strLine})({oldM.dOdd2}) -> New FT-AH({newM.strLine})({newM.dOdd2})");
                    }
                }
            }

            return results;
        }
        public void updateMarket(string site, Market item)
        {
            if (!_sites.Contains(site) || item == null)
                return;
            Mutex curMutex = mutexDic[site];
            try
            {
                curMutex.WaitOne();
                List<MatchItem> matches = dataDic[site];
                MatchItem curMatch = matches.Find(o => o.matchId == item.matchId);
                if (curMatch != null)
                {
                    curMatch.updateTime = DateTime.Now;
                    Market curMarket = curMatch.markets.Find(o => o.isSameMarket(item));
                    if (curMarket == null)
                        curMatch.markets.Add(item);
                    else
                    {
                        curMarket.dOdd1 = item.dOdd1;
                        curMarket.dOdd2 = item.dOdd2;
                        curMarket.strLine = item.strLine;
                        curMarket.status = item.status;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"updateMarket error - {ex.Message}");
            }
            finally
            {
                curMutex.ReleaseMutex();
            }
        }
        public void endMatch(string site, string matchId)
        {
            if (!_sites.Contains(site) || string.IsNullOrEmpty(matchId))
                return;
            Mutex curMutex = mutexDic[site];
            try
            {
                curMutex.WaitOne();
                List<MatchItem> matches = dataDic[site];
                List<MatchItem> endMatchs = matches.Where(o => o.matchId == matchId).ToList();
                if (endMatchs.Count > 0)
                    dataDic[site] = matches.Except(endMatchs).ToList();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"endMatch error - {ex.Message}");
            }
            finally
            {
                curMutex.ReleaseMutex();
            }
        }
        public void endMarket(string site, string marketId)
        {
            if (!_sites.Contains(site) || string.IsNullOrEmpty(marketId))
                return;
            Mutex curMutex = mutexDic[site];
            try
            {
                curMutex.WaitOne();
                List<MatchItem> matches = dataDic[site];
                MatchItem curMatch = matches.Find(o => o.markets.Find(c => c.marketId == marketId) != null);
                if (curMatch != null)
                {
                    List<Market> removeMarkets = curMatch.markets.Where(o => o.marketId == marketId).ToList();
                    if (removeMarkets.Count > 0)
                        curMatch.markets = curMatch.markets.Except(removeMarkets).ToList();
                    curMatch.updateTime = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"endMarket error - {ex.Message}");
            }
            curMutex.ReleaseMutex();
        }
        private void checkRelation(string site1, string site2)
        {
            try
            {
                List<MatchItem> list1 = getMatches(site1);
                List<MatchItem> list2 = getMatches(site2);
                if (list1.Count == 0 || list2.Count == 0)
                    return;
                List<BindItem> binds = getBinding(site1, site2);
                foreach (MatchItem one in list1)
                {
                    if (binds.Find(o => o.matchId1 == one.matchId) != null)
                        continue;
                    MatchItem findItem = new MatchItem();
                    findItem = null;
                    findItem = list2.Find(o => Utils.isSameMatch(one.leagueName, one.home, one.away, one.startTime.Hour, o.leagueName, o.home, o.away, o.startTime.Hour) == true);
                    if (findItem != null)
                    {
                        binds.Add(new BindItem(one.matchId, findItem.matchId));
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"checkRelation error - {ex.Message}");
            }
        }
        public bool isValidPair(string site1, string site2)
        {
            if (site1 == site2)
                return false;
            return true;
        }
        public void checkCandidate(string site, Market item)
        {
            try
            {
                if (item.status == "suspend")
                    return;
                candMutex.WaitOne();
                //string dir = $"{Application.StartupPath}\\relateCheck";
                //if (!Directory.Exists(dir))
                //    Directory.CreateDirectory(dir);
                //string path = $"{dir}\\Superodd_{DateTime.Now.ToString("yyyyMMdd")}.txt";
                //string content = "";

                foreach (string siteOne in _sites)
                {
                    if (!isValidPair(site, siteOne))
                        continue;
                    MatchItem master = dataDic[site].Find(o => o.matchId == item.matchId);
                    if (master == null)
                        continue;
                    MatchItem slave = getSlaveMatch(site, siteOne, master);
                    if (slave == null)
                        continue;
                    //if (master.updateTime.AddSeconds(Setting.Instance.candidateTime) < DateTime.Now || master.updateTime.AddSeconds(Setting.Instance.candidateTime) < DateTime.Now)
                    //    continue;
                    Market slaveMarket = slave.markets.Find(o => o.nType == item.nType && o.period == item.period && o.strLine == item.strLine);
                    if (slaveMarket == null || slaveMarket.status == "suspend")
                        continue;

                    double dProfit = Utils.calcProfit(item.dOdd1, slaveMarket.dOdd2);
                    //content = $"{master.strMatchName} - {item.strMarketName} ---> {item.dOdd1}/{item.dOdd2}  {slaveMarket.dOdd1}/{slaveMarket.dOdd2} -- {slave.strMatchName} ----DATETIME---- {DateTime.Now}\n";
                    //using (FileStream fs = File.Open(path, FileMode.Append, FileAccess.Write))
                    //{
                    //    using (StreamWriter writer = new StreamWriter(fs))
                    //        writer.WriteLine($"{content}");
                    //}
                    //Trace.WriteLine($"{master.strMatchName} - {item.strMarketName} ---> {item.dOdd1}/{item.dOdd2}  {slaveMarket.dOdd1}/{slaveMarket.dOdd2}");
                    if (dProfit > 0)
                    {
                        PlaceBetCandidate candidate = new PlaceBetCandidate();
                        candidate.league = master.leagueName;
                        candidate.home = master.home;
                        candidate.away = master.away;
                        BetData betData1 = new BetData(item, $"{master.leagueName}-{master.strMatchName}", master.matchId, site, 0);
                        BetData betData2 = new BetData(slaveMarket, $"{slave.leagueName}-{slave.strMatchName}", slave.matchId, siteOne, 1);
                        candidate.strScore = master.strScore;
                        candidate.strTime = $"{(site.Contains("MB") ? master.minute : slave.minute)}";
                        candidate.strMasterCode = site;
                        candidate.betMaster = betData1;
                        candidate.strSlaveCode = siteOne;
                        candidate.betSlave = betData2;
                        candidate.createTime = DateTime.Now;
                        //if (setItem.bGreaterFirst)
                        //{

                        //}
                        //else
                        //{
                        //	candidate.strSlaveCode = strMasterCode;
                        //	candidate.betSlave = betData1;
                        //	candidate.strMasterCode = strSlaveCode;
                        //	candidate.betMaster = betData2;
                        //}
                        candidate.dProfit = dProfit * 100;
                        updateCandidate(candidate);
                    }

                    dProfit = Utils.calcProfit(item.dOdd2, slaveMarket.dOdd1);

                     if (dProfit > 0)
                    {
                        PlaceBetCandidate candidate = new PlaceBetCandidate();
                        candidate.league = master.leagueName;
                        candidate.home = master.home;
                        candidate.away = master.away;
                        BetData betData1 = new BetData(item, $"{master.leagueName}-{master.strMatchName}", master.matchId, site, 1);
                        BetData betData2 = new BetData(slaveMarket, $"{slave.leagueName}-{slave.strMatchName}", slave.matchId, siteOne, 0);
                        candidate.strScore = master.strScore;
                        candidate.strTime = $"{(!site.Contains("MB") ? master.minute : slave.minute)}";
                        candidate.strMasterCode = siteOne;
                        candidate.betMaster = betData2;
                        candidate.strSlaveCode = site;
                        candidate.betSlave = betData1;
                        candidate.createTime = DateTime.Now;
                        //if (setItem.bGreaterFirst)
                        //{

                        //}
                        //else
                        //{
                        //	candidate.strMasterCode = strMasterCode;
                        //	candidate.betMaster = betData1;
                        //	candidate.strSlaveCode = strSlaveCode;
                        //	candidate.betSlave = betData2;
                        //}
                        candidate.dProfit = dProfit * 100;
                        updateCandidate(candidate);
                    }
                }

            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            finally
            {
                candMutex.ReleaseMutex();
            }
        }
        private bool isValidCandidate(PlaceBetCandidate cand)
        {
            bool ret = false;
            try
            {
                if (cand == null || cand.betMaster == null || cand.betSlave == null)
                    return false;
                MatchItem masterMatch = dataDic[cand.strMasterCode].Find(o => o.matchId == cand.betMaster.eventId);
                MatchItem slaveMatch = dataDic[cand.strSlaveCode].Find(o => o.matchId == cand.betSlave.eventId);
                if (masterMatch == null || slaveMatch == null)
                    return false;
                Market masterMarket = masterMatch.markets.Find(o => o.nType == cand.betMaster.nType && o.period == cand.betMaster.period && o.strLine == cand.betMaster.strHandicap);
                Market slaveMarket = slaveMatch.markets.Find(o => o.nType == cand.betSlave.nType && o.period == cand.betSlave.period && o.strLine == cand.betSlave.strHandicap);
                if (masterMarket == null || slaveMarket == null)
                    return false;
                if (masterMarket.status == "suspend" || slaveMarket.status == "suspend")
                    return false;
                cand.strScore = masterMatch.strScore;
                cand.strTime = $"{(cand.strMasterCode.Contains("MB") ? masterMatch.minute : slaveMatch.minute)}";
                double masterOdd = cand.betMaster.nSide == 0 ? masterMarket.dOdd1 : masterMarket.dOdd2;
                double slaveOdd = cand.betSlave.nSide == 0 ? slaveMarket.dOdd1 : slaveMarket.dOdd2;
                cand.betMaster.dOdds = masterOdd;
                cand.betSlave.dOdds = slaveOdd;
                cand.updateProfit();
                if (cand.dProfit <= 0)
                    return false;
                if (cand.strSlaveCode == "SP")
                {
                    cand.strSlaveCode = cand.strMasterCode;
                    cand.strMasterCode = "SP";
                    BetData tmpData = cand.betMaster;
                    cand.betMaster = cand.betSlave;
                    cand.betSlave = tmpData;
                }
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"isValidCandidate error - {ex.Message}");
            }
            return ret;
        }
        public List<PlaceBetCandidate> getCandidate()
        {
            try
            {
                candMutex.WaitOne();
                candidates = candidates.Where(o => isValidCandidate(o)).ToList();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"getCandidate error - {ex.Message}");
            }
            finally
            {
                candMutex.ReleaseMutex();
            }
            return candidates;
        }
        private void updateCandidate(PlaceBetCandidate cand)
        {
            try
            {
                if (cand == null || cand.betMaster == null || cand.betSlave == null)
                    return;
                if (cand.dProfit < Setting.Instance.percentMin)
                    return;
                if (cand.strSlaveCode == "SP")
                {
                    cand.strSlaveCode = cand.strMasterCode;
                    cand.strMasterCode = "SP";
                    BetData tmpData = cand.betMaster;
                    cand.betMaster = cand.betSlave;
                    cand.betSlave = tmpData;
                }
                candMutex.WaitOne();
                PlaceBetCandidate findCand = candidates.Find(o => o.strMasterCode == cand.strMasterCode && o.strSlaveCode == cand.strSlaveCode && o.betMaster.isSameBet(cand.betMaster) && o.betSlave.isSameBet(cand.betSlave));
                if (findCand != null)
                {
                    findCand.betMaster = cand.betMaster;
                    findCand.betSlave = cand.betSlave;
                    findCand.strTime = cand.strTime;
                    findCand.strScore = cand.strScore;
                    findCand.betMaster.dOdds = cand.betMaster.dOdds;
                    findCand.betSlave.dOdds = cand.betSlave.dOdds;
                    findCand.createTime = DateTime.Now;
                }
                else
                {
                    cand.index = ++candIndex;
                    candidates.Add(cand);
                    if (!Directory.Exists(tipDir))
                        Directory.CreateDirectory(tipDir);
                    string filePath = $"{tipDir}\\{DateTime.Now.ToString("yyyyMMdd")}.txt";
                    using (FileStream fs = File.Open(filePath, FileMode.Append, FileAccess.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(fs))
                            writer.WriteLine($"[{DateTime.Now.ToString("hh:mm:ss")}] {JsonConvert.SerializeObject(cand)}");
                    }
                    //if (cand.strMasterCode == "PB" || cand.strSlaveCode == "PB")
                    //{
                    //    BetData data = cand.strMasterCode == "PB" ? cand.betMaster : cand.betSlave;
                    //    if (data.nSide == 1 && data.nType % 2 == 0 && data.strHandicap != "0")
                    //    {
                    //        Trace.WriteLine($"New PB candidate - {data.eventName}({data.eventId}) - {data.strMarketName}({data.sectionId}) - {data.dOdds}");
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"updateCandidate error - {ex.Message}");
            }
            finally
            {
                candMutex.ReleaseMutex();
            }
        }
        public MatchItem getSlaveMatch(string masterCode, string slaveCode, MatchItem master)
        {
            MatchItem ret = null;
            try
            {
                RelationItem relItem = _relations.Find(o => o.site1 == masterCode && o.site2 == slaveCode || o.site1 == slaveCode && o.site2 == masterCode);
                if (relItem == null)
                    return ret;
                bool positive = masterCode == relItem.site1;
                BindItem bind = positive ? relItem.bindList.Find(o => o.matchId1 == master.matchId) : relItem.bindList.Find(o => o.matchId2 == master.matchId);
                if (bind == null)
                    return ret;
                string slaveMatchId = positive ? bind.matchId2 : bind.matchId1;
                ret = dataDic[slaveCode].Find(o => o.matchId == slaveMatchId);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"getSlaveMatch erorr - {ex.Message}");
            }
            return ret;
        }
        public List<RelationItem> getRelation()
        {
            return _relations;
        }
        public string copyBinding()
        {
            string ret = JsonConvert.SerializeObject(_relations);
            Clipboard.SetText(ret);
            return ret;
        }
    }
}
