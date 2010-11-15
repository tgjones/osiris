using System;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Terrain.GeoMipMapping
{
	/// <summary>
	/// Summary description for TerrainVertex.
	/// </summary>
	public static class TerrainVertex
	{
		public static readonly VertexElement[] VertexElements;

		static TerrainVertex()
		{
			// set vertex definition
			VertexElements = new VertexElement[]
			{
				// stream 0
				new VertexElement(0, 0, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.Position, 0),

				// stream 1
				new VertexElement(1, 0, VertexElementFormat.Single, VertexElementMethod.Default, VertexElementUsage.Position, 1)
			};
		}

		public struct Shared
		{
			public float X;
			public float Y;

			public static int SizeInBytes
			{
				get { return 2 * sizeof(float); }
			}

			public Shared(float x, float y)
			{
				X = x;
				Y = y;
			}
		}

		public struct Local
		{
			public float Z;

			public static int SizeInBytes
			{
				get { return sizeof(float); }
			}

			public Local(float z)
			{
				Z = z;
			}
		}
	}
}
