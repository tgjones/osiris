using System;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Sky
{
	public struct SkyDomeVertex
	{
		public static readonly VertexElement[] VertexElements;

		public float X;
		public float Y;
		public float Z;

		static SkyDomeVertex()
		{
			// set vertex definition
			VertexElements = new VertexElement[]
			{
				new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
			};
		}

		public static int SizeInBytes
		{
			get { return 3 * sizeof(float); }
		}

		public SkyDomeVertex(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public override string ToString()
		{
			return string.Format("Pos: {0}, {1}, {2}", X, Y, Z);
		}
	}
}
