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
    public partial class frmTest : Form
    {
        public frmTest()
        {
            InitializeComponent();
        }

        private void btnURL_Click(object sender, EventArgs e)
        {
            Setting.Instance.browser.gotoUrl(txtData.Text);
        }

        private void btnScript_Click(object sender, EventArgs e)
        {
            txtData.Text = Setting.Instance.browser.executeScript(txtData.Text);
        }
    }
}
