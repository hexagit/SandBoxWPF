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
	public class PixelShaders : Shaders<PixelShader>
	{
		public static PixelShaders Instance { get; } = new PixelShaders();
		protected override string Profile { get { return "ps_5_0"; } }

		protected override PixelShader CreateShader(CompilationResult shaderBytecode)
		{
			return new PixelShader(D3DSystem.Instance.Deivce, shaderBytecode);
		}
	}
}
