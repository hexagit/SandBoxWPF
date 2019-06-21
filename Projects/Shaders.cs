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
	public abstract class Shaders<T> : IDisposable
		where T : class, IDisposable
	{
		internal class ShaderObject : IDisposable
		{
			public string Name { get { return name; } }
			private string name;
			public int Index { get { return index; } }
			private int index;
			public CompilationResult ByteCode { get { return byteCode; } }
			CompilationResult byteCode = null;
			public T Shader { get { return shader; } }
			T shader = null;

			public ShaderObject(int index, string name, CompilationResult byteCode, T shader)
			{
				this.index = index;
				this.name = name;
				this.byteCode = byteCode;
				this.shader = shader;
			}
			public void Dispose()
			{
				Utilities.Dispose(ref byteCode);
				Utilities.Dispose(ref shader);
			}
		}
		private List<ShaderObject> shaders = new List<ShaderObject>();
		public void Dispose()
		{
			for (int i = 0; i < shaders.Count; ++i)
			{
				var shaerObject = shaders[i];
				Utilities.Dispose(ref shaerObject);
			}
			shaders.Clear();
		}
		public int GetShaderIndex(string name)
		{
			return shaders?.Where(s => string.Compare(name, s.Name, true) == 0).Select(s => s.Index).FirstOrDefault() ?? -1;
		}
		public T GetShader(int index)
		{
			return shaders[index].Shader;
		}
		public CompilationResult GetByteCode(int index)
		{
			return shaders[index].ByteCode;
		}
		abstract protected T CreateShader(CompilationResult shaderBytecode);
		abstract protected string Profile { get; }
		public int Load(string filePath, string name)
		{
			var shader = shaders?.Where(s => string.Compare(name, s.Name, true) == 0).FirstOrDefault();
			if (shader != null)
			{
				return shader.Index;
			}
			int shaderIndex = shaders.Count;
			var shaderBytecode = ShaderBytecode.CompileFromFile(filePath, "main", Profile, ShaderFlags.OptimizationLevel3);
			var shaderObject = CreateShader(shaderBytecode);
			shaders.Add(new ShaderObject(shaderIndex, name, shaderBytecode, shaderObject));
			return shaderIndex;
		}
	}
}
