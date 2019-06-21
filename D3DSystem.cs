using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Device = SharpDX.Direct3D11.Device;

namespace SandBox
{
	public class D3DRenderTarget : IDisposable
	{
		Texture2D texture = null;
		RenderTargetView rtv = null;
		ShaderResourceView srv = null;
		UnorderedAccessView uav = null;
		public Texture2D Texture { get { return texture; } }
		public RenderTargetView RTV { get { return rtv; } }
		public ShaderResourceView SRV { get{ return srv; } }
		public UnorderedAccessView UAV { get{ return uav; } }
		public D3DRenderTarget(
			Texture2D texture,
			RenderTargetView rtv,
			ShaderResourceView srv,
			UnorderedAccessView uav)
		{
			this.texture = texture;
			this.rtv = rtv;
			this.srv = srv;
			this.uav = uav;
		}
		public void Dispose()
		{
			Utilities.Dispose(ref uav);
			Utilities.Dispose(ref srv);
			Utilities.Dispose(ref rtv);
			Utilities.Dispose(ref texture);
		}
	}
	public class D3DDepthStencil : IDisposable
	{
		Texture2D texture = null;
		DepthStencilView dsv = null;
		ShaderResourceView srv = null;
		public Texture2D Texture { get { return texture; } }
		public DepthStencilView DSV { get { return dsv; } }
		public ShaderResourceView SRV { get { return srv; } }
		public D3DDepthStencil(
			Texture2D texture,
			DepthStencilView dsv,
			ShaderResourceView srv)
		{
			this.texture = texture;
			this.dsv = dsv;
			this.srv = srv;
		}
		public void Dispose()
		{
			Utilities.Dispose(ref srv);
			Utilities.Dispose(ref dsv);
			Utilities.Dispose(ref texture);
		}
	}
	public class D3DSystem : IDisposable
	{
		public static D3DSystem Instance { get; private set; } = new D3DSystem();
		public const int BackBufferCount = 2;

		public Device Deivce { get { return device; } }
		private Device device = null;
		public DeviceContext ImmediateContext { get { return immediateContext; } }
		private DeviceContext immediateContext = null;

		SharpDX.DXGI.Device dxgi = null;
		SharpDX.DXGI.Factory dxgiFactory = null;


		private D3DSystem()
		{
			device = new Device(DriverType.Hardware, DeviceCreationFlags.Debug);
			immediateContext = device.ImmediateContext;

			dxgi = device.QueryInterface<SharpDX.DXGI.Device> ();
			dxgiFactory = dxgi.Adapter.GetParent<SharpDX.DXGI.Factory>();
		}
		public void Dispose()
		{
			Utilities.Dispose(ref immediateContext);
			Utilities.Dispose(ref device);
		}

		public SwapChain CreateSwapChain(IntPtr hWnd, int width, int height)
		{
			var desc = new SwapChainDescription()
			{
				BufferCount = BackBufferCount,
				ModeDescription = new ModeDescription(width, height,
					new Rational(60, 1), Format.R8G8B8A8_UNorm_SRgb),
				IsWindowed = true,
				OutputHandle = hWnd,
				SampleDescription = new SampleDescription(1, 0),
				SwapEffect = SwapEffect.Discard,
				Usage = Usage.RenderTargetOutput
			};
			return new SwapChain(dxgiFactory, device, desc);
		}
		public D3DRenderTarget ResizeSwapChain(SwapChain swapChain, int width, int height)
		{
			swapChain.ResizeBuffers(BackBufferCount, width, height, Format.Unknown, SwapChainFlags.None);
			var texture = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
			var rtv = new RenderTargetView(device, texture);
			return new D3DRenderTarget(texture, rtv, null, null);
		}
		public D3DDepthStencil CreateDepthStencil(int width, int height)
		{
			var description = new Texture2DDescription();
			description.Width = width;
			description.Height = height;
			description.ArraySize = 1;
			description.BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource;
			description.CpuAccessFlags = CpuAccessFlags.None;
			description.MipLevels = 1;
			description.OptionFlags = ResourceOptionFlags.None;
			description.SampleDescription.Count = 1;
			description.Usage = ResourceUsage.Default;
			description.Format = Format.R32_Typeless;

			var texture = new Texture2D(device, description);
			var depthDescription = new DepthStencilViewDescription();
			depthDescription.Dimension = DepthStencilViewDimension.Texture2D;
			depthDescription.Flags = DepthStencilViewFlags.None;
			depthDescription.Format = Format.D32_Float;
			depthDescription.Texture2D.MipSlice = 0;
			var depthStencil = new DepthStencilView(device, texture, depthDescription);

			var srvDescription = new ShaderResourceViewDescription();
			srvDescription.Format = Format.R32_Float;
			srvDescription.Dimension = ShaderResourceViewDimension.Texture2D;
			srvDescription.Texture2D.MipLevels = 1;
			srvDescription.Texture2D.MostDetailedMip = 0;
			var depthSrv = new ShaderResourceView(device, texture, srvDescription);
			return new D3DDepthStencil(texture, depthStencil, depthSrv);
		}
	}
}
