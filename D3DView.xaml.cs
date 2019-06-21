using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Windows.Threading;

namespace SandBox
{
	/// <summary>
	/// D3DView.xaml の相互作用ロジック
	/// </summary>
	public partial class D3DView : UserControl
	{
		D3DViewOnForm d3dOnForm = null;
		D3DRenderScreen d3dScreen = null;
		D3DViewModel viewModel = null;
		public D3DView()
		{
			InitializeComponent();
			if (DesignerProperties.GetIsInDesignMode(this))
			{
				return;
			}
			viewModel = new D3DViewModel();
			DataContext = viewModel;
			Dispatcher.ShutdownStarted += (sender, e) => {
				(DataContext as D3DViewModel)?.Dispose();
				DataContext = null;
			};
			Loaded += (sender, e)=>
			{
				if (DesignerProperties.GetIsInDesignMode(this))
				{
					return;
				}
				if (FormHost.Child == null)
				{
					d3dOnForm = new D3DViewOnForm();
					d3dScreen = new D3DRenderScreen(d3dOnForm.Handle, (int)ActualWidth, (int)ActualHeight);
					viewModel.RenderScreen = d3dScreen;
					FormHost.Child = d3dOnForm;

					var timer = new DispatcherTimer();
					timer.Tick += new EventHandler(OnRefresh);
					timer.Interval = new TimeSpan(0, 0, 0, 0, 16);
					timer.Start();
				}
			};
		}
		public void OnRefresh(object sender, EventArgs e)
		{
			d3dOnForm.Refresh();
		}

		private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (d3dScreen != null)
			{
				d3dScreen.Width.Value = (int)ActualWidth;
				d3dScreen.Height.Value = (int)ActualHeight;
			}
		}
	}
}
