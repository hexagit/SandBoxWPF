using Assimp;
using Assimp.Configs;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace SandBox
{
	public class FileMeshRenderObject : IRenderObject
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct Vertex
		{
			public Vector3 Position;
			public Half4 Normal;
		}
		internal class Cluster : IDisposable
		{
			Buffer vertexBuffer = null;
			Buffer indexBuffer = null;
			public Buffer VertexBuffer { get { return vertexBuffer; } set { Utilities.Dispose(ref vertexBuffer);  vertexBuffer = value; } }
			public Buffer IndexBuffer { get { return indexBuffer; } set { Utilities.Dispose(ref indexBuffer); indexBuffer = value; } }
			public VertexBufferBinding VertexBufferBinding { get; set; }
			public int IndicesCount { get; set; } = 0;

			public void Dispose()
			{
				Utilities.Dispose(ref vertexBuffer);
				Utilities.Dispose(ref indexBuffer);
			}
		}
		string filePath;
		List<Cluster> clusters = new List<Cluster>();

		public FileMeshRenderObject(string filePath)
		{
			this.filePath = filePath;
			using (var importer = new AssimpContext())
			{
				importer.SetConfig(new NoSkeletonMeshesConfig(true));
				var model = importer.ImportFile(filePath, PostProcessPreset.TargetRealTimeMaximumQuality);
				foreach (var mesh in model.Meshes)
				{
					SetupMesh(mesh);
				}
			}
		}
		void SetupMesh(Mesh mesh)
		{
			var cluster = new Cluster();
			// count total indices as triangle face
			int numberOfIndex = 0;
			foreach (var face in mesh.Faces)
			{
				numberOfIndex += (face.IndexCount - 2) * 3;
			}
			cluster.IndicesCount = numberOfIndex;
			int sizeOfByte = numberOfIndex * Utilities.SizeOf<int>();
			using (var stream = new DataStream(sizeOfByte, true, true))
			{
				foreach (var face in mesh.Faces)
				{
					int numberOfTriangle = face.IndexCount - 2;
					for (int i = 0; i < numberOfTriangle; ++i)
					{
						stream.Write(face.Indices[0]);
						stream.Write(face.Indices[i + 1]);
						stream.Write(face.Indices[i + 2]);
					}
				}
				stream.Position = 0;
				cluster.IndexBuffer = new Buffer(D3DSystem.Instance.Deivce, stream, sizeOfByte, ResourceUsage.Default, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
			}
			sizeOfByte = mesh.VertexCount * Utilities.SizeOf<Vertex>();
			using (var stream = new DataStream(sizeOfByte, true, true))
			{
				for (int i = 0; i < mesh.VertexCount; ++i)
				{
					var vertex = new Vertex();
					var position = mesh.Vertices[i];
					vertex.Position.X = position.X;
					vertex.Position.Y = position.Y;
					vertex.Position.Z = position.Z;
					var normal = mesh.Normals[i];
					vertex.Normal.X = normal.X;
					vertex.Normal.Y = normal.Y;
					vertex.Normal.Z = normal.Z;
					vertex.Normal.W = 0;
					stream.Write(vertex);
				}
				stream.Position = 0;
				cluster.VertexBuffer = new Buffer(D3DSystem.Instance.Deivce, stream, sizeOfByte, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
			}
			cluster.VertexBufferBinding = new VertexBufferBinding(cluster.VertexBuffer, Utilities.SizeOf<Vertex>(), 0);
			clusters.Add(cluster);
		}
		public void Dispose()
		{
			for (int i = 0; i < clusters?.Count; ++i)
			{
				var cluster = clusters[i];
				Utilities.Dispose(ref cluster);
			}
			clusters?.Clear();
		}

		public void Draw()
		{
			var context = D3DSystem.Instance.ImmediateContext;
			foreach (var cluster in clusters)
			{
				context.InputAssembler.SetIndexBuffer(cluster.IndexBuffer, Format.R32_UInt, 0);
				context.InputAssembler.SetVertexBuffers(0, cluster.VertexBufferBinding);
				context.DrawIndexed(cluster.IndicesCount, 0, 0);
			}
		}
	}
}
