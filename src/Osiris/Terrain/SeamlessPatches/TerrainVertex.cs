using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Terrain.SeamlessPatches
{
	[StructLayout(LayoutKind.Sequential)]
	public struct TerrainVertex
	{
		public float X;
		public float Z;

		public static readonly int SizeInBytes = sizeof(float) * 2;

		public static readonly VertexElement[] VertexElements = new VertexElement[]
		{
			new VertexElement(0, 0, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.Position, 0),
		};

		public TerrainVertex(float x, float z)
		{
			X = x;
			Z = z;
		}

		public override string ToString()
		{
			return string.Format("Pos: {0}, {1}", X, Z);
		}
	}
}
