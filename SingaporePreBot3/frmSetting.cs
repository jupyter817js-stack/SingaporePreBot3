using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SingaporePreBot3
{
    public partial class frmSetting : Form
    {
        public frmSetting()
        {
            InitializeComponent();
        }

        private void frmSetting_Load(object sender, EventArgs e)
        {
            txtDomainDafabet.Text = Setting.Instance.domainSingapore;
            txtUsernameDafabet.Text = Setting.Instance.usernameSingapore;
            txtPasswordDafabet.Text = Setting.Instance.passwordSingapore;
            txtDomainPinnacle.Text = Setting.Instance.domainSuperodd;
            txtUsernamePinnacle.Text = Setting.Instance.usernameSuperodd;
            txtPasswordPinnacle.Text = Setting.Instance.passwordSuperodd;
            numStartTime.Text = Setting.Instance.startTime.ToString();
            numEndTime.Text = Setting.Instance.endTime.ToString();
            numDafabet.Value = (decimal)Setting.Instance.singaporeTime;
            numPinnacle.Value = (decimal)Setting.Instance.superoddTime;
            numCandidate.Value = (decimal)Setting.Instance.candidateTime;
            txtPercentMin.Text = Setting.Instance.percentMin.ToString();
            txtStake.Text = string.Join(Environment.NewLine, Setting.Instance.stakeList.Select(o => $"{o.min}:{o.max}:{o.stake}:{o.count}"));
            chkPlaySound.Checked = Setting.Instance.playSound;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Setting.Instance.domainSingapore = txtDomainDafabet.Text;
            Setting.Instance.usernameSingapore = txtUsernameDafabet.Text;
            Setting.Instance.passwordSingapore = txtPasswordDafabet.Text;
            Setting.Instance.domainSuperodd = txtDomainPinnacle.Text;
            Setting.Instance.usernameSuperodd = txtUsernamePinnacle.Text;
            Setting.Instance.passwordSuperodd = txtPasswordPinnacle.Text;
            Setting.Instance.startTime = (int)numStartTime.Value;
            Setting.Instance.endTime = (int)numEndTime.Value;
            Setting.Instance.singaporeTime = (int)numDafabet.Value;
            Setting.Instance.superoddTime = (int)numPinnacle.Value;
            Setting.Instance.candidateTime = (int)numCandidate.Value;
            Setting.Instance.percentMin = Utils.ParseToDouble(txtPercentMin.Text);
            Setting.Instance.stakeList = parseStake();
            Setting.Instance.playSound = chkPlaySound.Checked;
            Setting.Instance.save();
        }

        private void numCandidate_ValueChanged(object sender, EventArgs e)
        {
            Setting.Instance.candidateTime = (int)numCandidate.Value;
        }

        private void numDafabet_ValueChanged(object sender, EventArgs e)
        {
            Setting.Instance.singaporeTime = (int)numDafabet.Value;
        }

        private void numPinnacle_ValueChanged(object sender, EventArgs e)
        {
            Setting.Instance.superoddTime = (int)numPinnacle.Value;
        }
        private List<StakeItem> parseStake()
        {
            List<StakeItem> ret = new List<StakeItem>();
            if (string.IsNullOrEmpty(txtStake.Text))
                return ret;
            try
            {
                string[] lines = txtStake.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    string[] tmp = line.Split(new string[] { ":", "-" }, StringSplitOptions.RemoveEmptyEntries);
                    if (tmp.Length == 4)
                        ret.Add(new StakeItem(Utils.ParseToDouble(tmp[0]), Utils.ParseToDouble(tmp[1]), Utils.ParseToDouble(tmp[2]), Utils.ParseToInt(tmp[3])));
                }
                if (ret.Count > 0)
                    ret = ret.OrderBy(o => o.stake).ToList();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
            }
            return ret;
        }

        private void label20_Click(object sender, EventArgs e)
        {
            Trace.WriteLine($"Stake - {JsonConvert.SerializeObject(Setting.Instance.stakeList)}");
        }
    }
}
