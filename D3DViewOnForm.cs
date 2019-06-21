using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SandBox
{
	public partial class D3DViewOnForm : UserControl
	{
		public D3DViewOnForm()
		{
			InitializeComponent();
			SetStyle(ControlStyles.Opaque | ControlStyles.UserPaint, true);
			UpdateStyles();
		}
	}
}
