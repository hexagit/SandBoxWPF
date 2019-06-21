using Reactive.Bindings;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SandBox
{
	public class D3DViewModel : IDisposable
	{
		private D3DRenderScreen renderScreen = null;
		public D3DRenderScreen RenderScreen
		{
			get
			{
				return renderScreen;
			}
			set
			{
				if (renderScreen != null) { throw new Exception("multiple set for RenderScreen."); }
				renderScreen = value;
				DrawType.Value = renderScreen.DrawType;
				DrawType.Subscribe(v => renderScreen.DrawType = v);
			}
		}
		public D3DViewModel()
		{
			DrawType = new ReactiveProperty<DrawTypes>();
		}
		public void Dispose()
		{
			Utilities.Dispose(ref renderScreen);
		}
		public IEnumerable<DrawTypes> DrawTypeList { get; } = Enum.GetValues(typeof(DrawTypes)).Cast<DrawTypes>();
		public ReactiveProperty<DrawTypes> DrawType { get; set; }
	}
}
