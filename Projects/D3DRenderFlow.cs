using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SandBox
{
	public interface IRenderObject : IDisposable
	{
		void Draw();
	}
	public class D3DRenderFlow : IDisposable
	{
		public static D3DRenderFlow Instance{get;} = new D3DRenderFlow();
		private List<IRenderObject> renderObjects = new List<IRenderObject>();

		int tempVertexShader = -1;
		int tempPixelShader = -1;
		InputLayout tempInputLayout = null;

		DepthStencilState depthState = null;
		private D3DRenderFlow()
		{
			tempVertexShader = VertexShaders.Instance.GetShaderIndex("test_vs");
			var vertexShaderBytecode = VertexShaders.Instance.GetByteCode(tempVertexShader);
			tempPixelShader = PixelShaders.Instance.GetShaderIndex("test_ps");
			tempInputLayout = new InputLayout(
				D3DSystem.Instance.Deivce,
				ShaderSignature.GetInputSignature(vertexShaderBytecode),
				new InputElement[]{
						new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0),
						new InputElement("NORMAL", 0, SharpDX.DXGI.Format.R16G16B16A16_Float, 12, 0),
				});
			DepthStencilStateDescription depthStencilStateDescription = new DepthStencilStateDescription();
			depthStencilStateDescription.IsDepthEnabled = true;
			depthStencilStateDescription.IsStencilEnabled = false;
			depthStencilStateDescription.DepthComparison = Comparison.Less;
			depthStencilStateDescription.DepthWriteMask = DepthWriteMask.All;
			depthState = new DepthStencilState(D3DSystem.Instance.Deivce, depthStencilStateDescription);
		}
		public void AddRenderObject(IRenderObject renderObject)
		{
			renderObjects.Add(renderObject);
		}
		public void Draw()
		{
			foreach (var screen in D3DRenderScreen.Screens)
			{
				screen.PrepareToDraw();

				D3DSystem.Instance.ImmediateContext.OutputMerger.DepthStencilState = depthState;

				DrawRenderObjects(screen);

				screen.Present();
			}
		}
		private void DrawRenderObjects(D3DRenderScreen screen)
		{
			var context = D3DSystem.Instance.ImmediateContext;
			context.InputAssembler.InputLayout = tempInputLayout;
			context.VertexShader.Set(VertexShaders.Instance.GetShader(tempVertexShader));
			context.PixelShader.Set(PixelShaders.Instance.GetShader(tempPixelShader));
			context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
			foreach (var renderObject in renderObjects)
			{
				renderObject.Draw();
			}
		}

		public void Dispose()
		{
			Utilities.Dispose(ref tempInputLayout);
			Utilities.Dispose(ref depthState);

		}
	}
}
