using Reactive.Bindings;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SandBox
{
	public enum DrawTypes
	{
		Solid,
		Wire,
	}
	public class DrawCamera
	{
		public Vector3 Position { get; set; } = new Vector3(5.0f, 5.0f, 10.0f);
		public Vector3 Target { get; set; } = new Vector3(0.0f, 4.0f, 0.0f);
		public float FieldOfView { get; set; } = 50.0f;
		public float NearPlaneDistance { get; set; } = 0.1f;
		public float FarPlaneDistance { get; set; } = 100.0f;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct ScreenConstantBufferType
	{
		public Matrix viewProjection;
		public Matrix view;
		public Matrix inverseViewProjection;
		public Matrix inverseView;
		public Vector2 screenSize;
		public Vector2 inverseScreenSize;
	}
	public class D3DRenderScreen : IDisposable
	{
		public static IEnumerable<D3DRenderScreen> Screens
		{
			get { return screens; }
		}
		private static List<D3DRenderScreen> screens = new List<D3DRenderScreen>();
		public SwapChain SwapChain { get { return swapChain; } }
		private SwapChain swapChain = null;
		private D3DRenderTarget d3dRenderTarget = null;
		private D3DDepthStencil d3dDepthStencil = null;

		public DrawCamera Camera { get { return Camera; } }
		private DrawCamera camera = new DrawCamera();

		public ReactiveProperty<int> Width { get; set; }
		public ReactiveProperty<int> Height { get; set; }
		private bool sizeChanged = true;

		SharpDX.Direct3D11.Buffer cbScreen = null;
		RasterizerState rasterizerState = null;

		public D3DRenderScreen(IntPtr hWnd, int width, int height)
		{
			swapChain = D3DSystem.Instance.CreateSwapChain(hWnd, width, height);

			cbScreen = new SharpDX.Direct3D11.Buffer(D3DSystem.Instance.Deivce, Utilities.SizeOf<ScreenConstantBufferType>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

			Width = new ReactiveProperty<int>(width);
			Height = new ReactiveProperty<int>(height);
			Width.Subscribe(_ => sizeChanged = true);
			Height.Subscribe(_ => sizeChanged = true);

			screens.Add(this);
		}
		public void Dispose()
		{
			screens.Remove(this);
			Utilities.Dispose(ref rasterizerState);
			Utilities.Dispose(ref cbScreen);
			Utilities.Dispose(ref d3dRenderTarget);
			Utilities.Dispose(ref d3dDepthStencil);
			Utilities.Dispose(ref swapChain);
		}

		public DrawTypes DrawType { get { return drawType; } set { drawTypeChanged = true; drawType = value; } }
		private DrawTypes drawType = DrawTypes.Solid;
		private bool drawTypeChanged = true;

		public void PrepareToDraw()
		{
			if (sizeChanged || (d3dRenderTarget == null) || (d3dDepthStencil == null))
			{
				Utilities.Dispose(ref d3dRenderTarget);
				Utilities.Dispose(ref d3dDepthStencil);
				d3dRenderTarget = D3DSystem.Instance.ResizeSwapChain(swapChain, Width.Value, Height.Value);
				d3dDepthStencil = D3DSystem.Instance.CreateDepthStencil(Width.Value, Height.Value);
				sizeChanged = false;
			}
			if (drawTypeChanged) {
				RasterizerStateDescription rasterizeStateDescription = new RasterizerStateDescription();
				rasterizeStateDescription.CullMode = CullMode.Back;
				rasterizeStateDescription.DepthBias = 0;
				rasterizeStateDescription.DepthBiasClamp = 0.0f;
				rasterizeStateDescription.FillMode = DrawType == DrawTypes.Solid ? FillMode.Solid : FillMode.Wireframe;
				rasterizeStateDescription.IsAntialiasedLineEnabled = false;
				rasterizeStateDescription.IsDepthClipEnabled = true;
				rasterizeStateDescription.IsFrontCounterClockwise = false;
				rasterizeStateDescription.IsMultisampleEnabled = false;
				rasterizeStateDescription.IsScissorEnabled = true;
				rasterizeStateDescription.SlopeScaledDepthBias = 0.0f;
				rasterizerState = new RasterizerState(D3DSystem.Instance.Deivce, rasterizeStateDescription);
				drawTypeChanged = false;
			}
			var context = D3DSystem.Instance.ImmediateContext;
			{
				var viewMatrix = Matrix.LookAtLH(camera.Position, camera.Target, Vector3.Up);
				var projectionMatrix = Matrix.PerspectiveFovLH(camera.FieldOfView / 180.0f * (float)Math.PI, (float)Width.Value / (float)Height.Value, camera.NearPlaneDistance, camera.FarPlaneDistance);
				var viewProjection = Matrix.Multiply(viewMatrix, projectionMatrix);
				var tempData = new ScreenConstantBufferType();
				tempData.inverseViewProjection = viewProjection;
				tempData.inverseViewProjection.Invert();
				tempData.inverseViewProjection.Transpose();
				viewProjection.Transpose();
				tempData.viewProjection = viewProjection;
				tempData.inverseView = viewMatrix;
				tempData.inverseView.Invert();
				tempData.inverseView.Transpose();
				viewMatrix.Transpose();
				tempData.view = viewMatrix;
				tempData.screenSize = new Vector2(Width.Value, Height.Value);
				tempData.inverseScreenSize = 1.0f / tempData.screenSize;

				context.UpdateSubresource(ref tempData, cbScreen);
			}
			{
				context.ClearRenderTargetView(d3dRenderTarget.RTV, new SharpDX.Mathematics.Interop.RawColor4(0.3f, 0.3f, 0.3f, 1.0f));
				context.ClearDepthStencilView(d3dDepthStencil.DSV, DepthStencilClearFlags.Depth, 1.0f, 0);
			}
			{
				context.OutputMerger.SetTargets(d3dDepthStencil.DSV, d3dRenderTarget.RTV);
				context.Rasterizer.State = rasterizerState;
				context.Rasterizer.SetViewport(0.0f, 0.0f, Width.Value, Height.Value);
				context.Rasterizer.SetScissorRectangle(0, 0, Width.Value, Height.Value);
			}
			{
				context.VertexShader.SetConstantBuffer(0, cbScreen);
			}
		}
		public void Present()
		{
			swapChain?.Present(0, PresentFlags.None);
		}
	}
}
