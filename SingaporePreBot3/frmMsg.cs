using System;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SingaporePreBot3
{
    public partial class frmMsg : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        SoundPlayer player;
        private string str;
        public frmMsg(string _str)
        {
            str = _str;
            InitializeComponent();
        }
        private void frmMsg_Load(object sender, EventArgs e)
        {
            lblMsg.Text = str;
            player = new SoundPlayer(Properties.Resources.popup);
            player.Play();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void item_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
    }
}
