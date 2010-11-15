using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics.Shaders;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Osiris.Graphics.SceneGraph.Nodes;

namespace Osiris.Graphics.SceneGraph.Geometries
{
	public class TriangleMesh : Geometry
	{
		private BoundingSphere _boundingSphere;

		internal override PrimitiveType Type
		{
			get { return PrimitiveType.TriangleList; }
		}

		public TriangleMesh(IServiceProvider serviceProvider, GeometryContainer geometryContainer,
			int baseVertex, int numVertices, int startIndex, int primitiveCount,
			BoundingSphere boundingSphere)
			: base(serviceProvider, geometryContainer, baseVertex, numVertices, startIndex, primitiveCount)
		{
			_boundingSphere = boundingSphere;
		}

		protected override void UpdateModelBound()
		{
			ModelBound = _boundingSphere;
		}
	}
}
