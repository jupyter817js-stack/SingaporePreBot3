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
    public partial class frmBlack : Form
    {
        public frmBlack()
        {
            InitializeComponent();
        }

        private void frmBlack_Load(object sender, EventArgs e)
        {
            txtBlack.Text = string.Join(Environment.NewLine, Setting.Instance.blackLeagues);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Setting.Instance.blackLeagues = txtBlack.Text.Split(new string[]{ "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}
