using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SingaporePreBot3
{
    public partial class frmRelation : Form
    {
        public frmRelation()
        {
            InitializeComponent();
        }

        private void frmRelation_Load(object sender, EventArgs e)
        {
            cmbPair.DataSource = RelationCtrl.Instance.getRelation().Select(o => $"{o.site1} - {o.site2}").ToList();
        }

        private void cmbPair_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateRelationData();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            updateRelationData();
        }

        private void btnMatch_Click(object sender, EventArgs e)
        {
            int relationIndex = cmbPair.SelectedIndex;
            if (relationIndex == -1)
                return;
            int oneIndex = dgvOne.SelectedCells != null ? dgvOne.SelectedCells[0].RowIndex : -1;
            int twoIndex = dgvTwo.SelectedCells != null ? dgvTwo.SelectedCells[0].RowIndex : -1;
            if (oneIndex == -1 || twoIndex == -1)
                return;
            MatchItem master = (MatchItem)dgvOne.Rows[oneIndex].Tag;
            MatchItem slave = (MatchItem)dgvTwo.Rows[twoIndex].Tag;

            RelationItem curRelation = RelationCtrl.Instance.getRelation()[relationIndex];
            curRelation.bindList.Add(new BindItem(master.matchId, slave.matchId));
            updateRelationData();
        }

        private void btnDisMatch_Click(object sender, EventArgs e)
        {
            int relationIndex = cmbPair.SelectedIndex;
            if (relationIndex == -1)
                return;
            int curIndex = dgvRelation.SelectedCells != null ? dgvRelation.SelectedCells[0].RowIndex : -1;
            if (curIndex == -1)
                return;
            List<string> ids = (List<string>)dgvRelation.Rows[curIndex].Tag;
            if (ids == null || ids.Count < 2)
                return;
            RelationItem curRelation = RelationCtrl.Instance.getRelation()[relationIndex];
            BindItem curBind = curRelation.bindList.Find(o => o.matchId1 == ids[0] && o.matchId2 == ids[1]);
            if (curBind == null)
                return;
            curRelation.bindList.Remove(curBind);
            updateRelationData();
        }

        private void updateRelationData()
        {
            int relationIndex = cmbPair.SelectedIndex;
            if (relationIndex == -1)
                return;
            RelationItem curRelation = RelationCtrl.Instance.getRelation()[relationIndex];
            this.Invoke(new Action(() =>
            {
                dgvOne.Rows.Clear();
                dgvTwo.Rows.Clear();
                dgvRelation.Rows.Clear();
                List<MatchItem> list1 = RelationCtrl.Instance.getMatches(curRelation.site1);
                List<MatchItem> list2 = RelationCtrl.Instance.getMatches(curRelation.site2);
                List<string> slaveIds = new List<string>();
                foreach (MatchItem master in list1)
                {
                    MatchItem slave = RelationCtrl.Instance.getSlaveMatch(curRelation.site1, curRelation.site2, master);
                    if (slave == null)
                    {
                        int rowIndex = dgvOne.Rows.Add(new object[]
                        {
                            master.leagueName,
                            master.strScore,
                            master.home,
                            master.away,
                        });
                        dgvOne.Rows[rowIndex].Tag = master;
                    }
                    else
                    {
                        slaveIds.Add(slave.matchId);
                        int rowIndex = dgvRelation.Rows.Add(new object[]
                        {
                            master.leagueName,
                            master.strScore,
                            master.home,
                            master.away,
                            slave.leagueName,
                            slave.strScore,
                            slave.home,
                            slave.away,
                        });
                        dgvRelation.Rows[rowIndex].Tag = new List<string> { master.matchId, slave.matchId };
                    }
                }
                foreach (MatchItem tmp2 in list2)
                {
                    if (!slaveIds.Contains(tmp2.matchId))
                    {
                        int rowIndex = dgvTwo.Rows.Add(new object[]
                        {
                            tmp2.leagueName,
                            tmp2.strScore,
                            tmp2.home,
                            tmp2.away,
                        });
                        dgvTwo.Rows[rowIndex].Tag = tmp2;
                    }
                }
                lblSite1.Text = $"{curRelation.site1} - {dgvOne.Rows.Count} matches";
                lblSite2.Text = $"{curRelation.site2} - {dgvTwo.Rows.Count} matches";
                lblRelation.Text = $"Relation count - {dgvRelation.Rows.Count}";
                //if (selectIndex > -1 && selectIndex < matches.Count)
                //    dgvMatch.CurrentCell = dgvMatch.Rows[selectIndex].Cells[0];
                //if (currentIndex > -1 && currentIndex < matches.Count)
                //    dgvMatch.FirstDisplayedScrollingRowIndex = currentIndex;
            }));
        }
    }
}
