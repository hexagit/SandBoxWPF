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
	public class VertexShaders : Shaders<VertexShader>
	{
		public static VertexShaders Instance { get; } = new VertexShaders();
		protected override string Profile { get { return "vs_5_0"; } }

		protected override VertexShader CreateShader(CompilationResult shaderBytecode)
		{
			return new VertexShader(D3DSystem.Instance.Deivce, shaderBytecode);
		}
	}
}
