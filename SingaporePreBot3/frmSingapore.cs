using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SingaporePreBot3
{
    public partial class frmSingapore : Form
    {
        public frmSingapore()
        {
            InitializeComponent();
        }

        public void updateMatch()
        {
            List<MatchItem> matches = RelationCtrl.Instance.getMatches("SP");
            this.Invoke(new Action(() => {
                int firstRow = dgvMatch.FirstDisplayedScrollingRowIndex;
                dgvMatch.Rows.Clear();
                for (int i = 0; i < matches.Count; i++)
                {
                    MatchItem one = matches[i];
                    int nRowIndex = dgvMatch.Rows.Add(new object[]
                        {
                            one.leagueName,
                            one.strMatchName,
                            one.strTime,
                            one.strMarket
                        });
                    dgvMatch.Rows[nRowIndex].Tag = one;
                }
                if (firstRow > -1 && firstRow < dgvMatch.Rows.Count)
                    dgvMatch.FirstDisplayedScrollingRowIndex = firstRow;
                lblNumber.Text = matches.Count.ToString();
            }));
        }

        private void dgvMatch_MouseClick(object sender, MouseEventArgs e)
        {
            if (!Setting.Instance.isTest)
                return;
            if (e.Button != MouseButtons.Right)
                return;
            int currentRowIndex = dgvMatch.HitTest(e.X, e.Y).RowIndex;
            if (currentRowIndex == -1)
                return;

            ContextMenu m = new ContextMenu();
            MatchItem match = dgvMatch.Rows[currentRowIndex].Tag as MatchItem;
            m.MenuItems.Add(new MenuItem("Copy Content", (sender1, e1) => copyContent(match)));
            if (Setting.Instance.isTest)
            {
                MenuItem betMenu = new MenuItem("TEST BET");
                foreach (Market curMarket in match.markets)
                {
                    betMenu.MenuItems.Add(new MenuItem($"{curMarket.strMarketName}-0/{curMarket.dOdd1}", (sender1, e1) => testBet(curMarket, 0, match.strMatchName)));
                    betMenu.MenuItems.Add(new MenuItem($"{curMarket.strMarketName}-1/{curMarket.dOdd2}", (sender1, e1) => testBet(curMarket, 1, match.strMatchName)));
                }
                m.MenuItems.Add(betMenu);
            }
            m.Show(dgvMatch, new Point(e.X, e.Y));
        }
        private void copyContent(MatchItem match)
        {
            try
            {
                string content = JsonConvert.SerializeObject(match);
                Clipboard.SetText(content);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }

        private void testBet(Market market, int side, string matchName)
        {
            try
            {
                BetData data = new BetData();
                data.dStake = 5;
                data.dOdds = side == 0 ? market.dOdd1 : market.dOdd2;
                data.period = market.period;
                data.nType = market.nType;              // 0- Goal Handicap 1- Goal OU 2- Corner Handicap 3- Corner OU
                data.strHandicap = market.strLine;      // Handicap or GoalLine Value like : -0.5
                data.nSide = side;                      // 0- Home or Over, 1-Away or Under
                data.eventId = market.matchId;
                data.sectionId = side == 0 ? market.outcome1 : market.outcome2;
                data.siteCode = "SP";
                data.eventName = matchName;
                Task.Run(() => {
                    AddSlipResult slipRet = Setting.Instance.spCtrl.addSlip(data);
                    Trace.WriteLine($"Betslip result - {JsonConvert.SerializeObject(slipRet)}");
                    BettingController.Instance.processAddSlipResult("SP", data, slipRet);
                    BetResult betRet = Setting.Instance.spCtrl.confirmBet(data);
                    BettingController.Instance.processBetResult("SP", data, betRet);
                    Trace.WriteLine($"Confirm result - {JsonConvert.SerializeObject(betRet)}");
                });
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }
    }
}
