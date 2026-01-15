using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Windows.Forms;

namespace SingaporePreBot3
{
    public partial class frmHistory : Form
    {
        public frmHistory()
        {
            InitializeComponent();
        }

        private void frmHistory_Load(object sender, EventArgs e)
        {

        }

		public void updateOrderList()
		{
			this.Invoke(new Action(() => {
				try
				{
					List<BetOrder> listOrders = Setting.Instance.getOrderList(true);
					if (listOrders != null && listOrders.Count > 0)
					{
						int selIndex = dgvHistory.CurrentCell?.RowIndex ?? -1;
						int scrollIndex = dgvHistory.FirstDisplayedScrollingRowIndex;
						lblBetNumber.Text = $"{listOrders.Count}";
						dgvHistory.Rows.Clear();
						foreach (BetOrder order in listOrders)
						{
							int nRowIndex = dgvHistory.Rows.Add(new object[]
							{
								order.strLeague,
								$"{order.strHome} - {order.strAway}",
								$"{order.strMasterBetType}",
								$"{order.strSlaveBetType}",
                                $"{order.masterOdd}",
								$"{order.slaveOdd}",
								order.dProfit.ToString("0.00"),
								$"{order.masterStake}",
								$"{order.getStatus()}{(!string.IsNullOrEmpty(order.reason) ? $"-{order.reason}" : "")}",
								order.orderTime.ToString("HH:mm"),
							});
							if (order.nOrderStatus == 1)        //성공
							{
								if (!order.betCandidate.isOneSide)
								{
									dgvHistory.Rows[nRowIndex].DefaultCellStyle.ForeColor = Color.Green;
									dgvHistory.Rows[nRowIndex].DefaultCellStyle.BackColor = Color.FromArgb(189, 223, 195);
								}
							}
							else if (order.nOrderStatus == -3)      ///실패
								dgvHistory.Rows[nRowIndex].DefaultCellStyle.ForeColor = Color.Black;
							else if (order.nOrderStatus == 2)       ///수동베팅함
								dgvHistory.Rows[nRowIndex].DefaultCellStyle.ForeColor = Color.Red;
							else if (order.nOrderStatus == -4)      ///베팅 취소됨
								dgvHistory.Rows[nRowIndex].DefaultCellStyle.ForeColor = Color.Pink;
							dgvHistory.Rows[nRowIndex].Tag = order;
						}
						if (dgvHistory.RowCount > scrollIndex && scrollIndex != -1)
							dgvHistory.FirstDisplayedScrollingRowIndex = scrollIndex;
						if (selIndex != -1)
							dgvHistory.CurrentCell = dgvHistory.Rows[selIndex].Cells[0];
						//if (scrollIndex != -1)
						//	gridOrder.FirstDisplayedScrollingRowIndex = scrollIndex;
					}
				}
				catch (Exception ex)
				{
					Trace.WriteLine($"updateOrderList error - {ex.Message}");
				}
			}));
		}

        private void label4_Click(object sender, EventArgs e)
        {
			try
			{
				string orderPath = $"{Global.LogDir}\\orders_{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt";
				File.WriteAllText(orderPath, JsonConvert.SerializeObject(Setting.Instance.orderList));
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"{MethodBase.GetCurrentMethod().Name} error - {ex.Message}");
			}
        }
    }
}
