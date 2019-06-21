using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
using System.Windows.Threading;

namespace SandBox
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			if (DesignerProperties.GetIsInDesignMode(this))
			{
				return;
			}
			Loaded += (sender, e) =>
			{
				String dataPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "data");
				{
					String filePath = Path.Combine(dataPath, "shaders", "test_ps.hlsl");
					PixelShaders.Instance.Load(filePath, "test_ps");
				}
				{
					String filePath = Path.Combine(dataPath, "shaders", "test_vs.hlsl");
					VertexShaders.Instance.Load(filePath, "test_vs");
				}
				{
					// the fbx data is https://www.turbosquid.com/3d-models/free-chubby-girl---ready-3d-model/805220
					String filePath = Path.Combine(dataPath, "meshes", "K_Chubby girl_Just do It_3D print.fbx");
					var fileMesh = new FileMeshRenderObject(filePath);
					D3DRenderFlow.Instance.AddRenderObject(fileMesh);
				}

				var timer = new DispatcherTimer();
				timer.Tick += new EventHandler(OnRender);
				timer.Interval = new TimeSpan(0, 0, 0, 0, 16);
				timer.Start();
			};
		}
		private void OnRender(object sender, EventArgs e)
		{
			try
			{
				D3DRenderFlow.Instance.Draw();
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
			}
		}
	}
}
