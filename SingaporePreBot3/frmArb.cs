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
    public partial class frmArb : Form
    {
        public frmArb()
        {
            InitializeComponent();
        }

        public void updateArb()
        {
            List<PlaceBetCandidate> arbList = Setting.Instance.candidateList;
            this.Invoke(new Action(() => {
                dgvArb.Rows.Clear();
                for (int i = 0; i < arbList.Count; i++)
                {
                    PlaceBetCandidate one = arbList[i];
                    int nRowIndex = dgvArb.Rows.Add(new object[]
                        {
                            one.league,
                            $"{one.home} vs {one.away}",
                            one.betMaster.strMarketName,
                            one.betSlave.strMarketName,
                            one.betMaster.dOdds,
                            one.betSlave.dOdds,
                            one.dProfit
                        });
                    dgvArb.Rows[nRowIndex].Tag = one;
                }
                lblNumber.Text = arbList.Count.ToString();
            }));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            updateArb();
        }
    }
}
