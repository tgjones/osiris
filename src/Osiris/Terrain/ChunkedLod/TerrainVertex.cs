using System;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Terrain.ChunkedLod
{
	public class TerrainVertex
	{
		public static readonly VertexElement[] VertexElements;

		static TerrainVertex()
		{
			// set vertex definition
			VertexElements = new VertexElement[]
			{
				// stream 0
				new VertexElement(0, 0, VertexElementFormat.Short2, VertexElementMethod.Default, VertexElementUsage.Position, 0),
			};
		}

		/// <summary>
		/// Because the footprint (x, z) coordinates are local, these do not require
		/// 32-bit precision, so we pack them into two shorts, requiring only 4 bytes
		/// per vertex.
		/// </summary>
		public struct Shared
		{
			public short X;
			public short Z;

			public static int SizeInBytes
			{
				get { return sizeof(short) * 2; }
			}

			public Shared(short x, short z)
			{
				X = x;
				Z = z;
			}

			public override string ToString()
			{
				return "X: " + X + ", Z: " + Z;
			}
		}
	}
}
