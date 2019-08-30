using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TabControlTest
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		private void ButtonFetchSize_Click(object sender, EventArgs e)
		{
			string fetchSize0 = this.Controls.Find($"textBoxFetchSize{0}", true)?[0]?.Text ?? "";
			string fetchSize1 = this.Controls.Find($"textBoxFetchSize{1}", true)?[0]?.Text ?? "";

			string fs0 = Properties.Settings.Default["FetchSize0"].ToString();
		}
	}
}
