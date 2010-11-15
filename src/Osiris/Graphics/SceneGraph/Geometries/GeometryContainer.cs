using System;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Graphics.SceneGraph.Geometries
{
	public class GeometryContainer
	{
		public VertexBuffer VertexBuffer { get; private set; }
		public IndexBuffer IndexBuffer { get; private set; }

		public GeometryContainer(VertexBuffer vertexBuffer, IndexBuffer indexBuffer)
		{
			VertexBuffer = vertexBuffer;
			IndexBuffer = indexBuffer;
		}
	}
}
