using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Diagnostics.Shapes
{
	internal static class Rectangle
	{
		private static readonly VertexPositionColor[] Vertices;
		private static readonly short[] SolidIndices;

		static Rectangle()
		{
			// These get moved to the real positions when they're drawn.
			Color color = Color.White;
			Vertices = new[]
			{
				new VertexPositionColor(Vector3.Zero, color),
				new VertexPositionColor(Vector3.Zero, color),
				new VertexPositionColor(Vector3.Zero, color),
				new VertexPositionColor(Vector3.Zero, color)
			};

			SolidIndices = new short[]
			{
				0, 1, 2, 1, 2, 3
			};
		}

		public static void DrawSolid(PrimitiveDrawer primitiveDrawer, Matrix cameraView, Matrix cameraProjection,
			Vector3[] corners, Color color)
		{
			if (corners == null || corners.Length != 4)
				throw new ArgumentException("corners");

			Vertices[0].Position = corners[0];
			Vertices[1].Position = corners[1];
			Vertices[2].Position = corners[2];
			Vertices[3].Position = corners[3];

			primitiveDrawer.Draw(Matrix.Identity, cameraView, cameraProjection, color, null,
				PrimitiveType.TriangleList, Vertices, SolidIndices, false);
		}
	}
}