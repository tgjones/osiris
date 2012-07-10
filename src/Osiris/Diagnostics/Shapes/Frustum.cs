using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Diagnostics.Shapes
{
	internal static class Frustum
	{
		private static readonly VertexPositionColor[] FrustumVertices;
		private static readonly short[] WireFrustumIndices;
		private static readonly Vector3[] Corners;

		static Frustum()
		{
			Color color = Color.White;

			// These get moved to the real positions when DrawWireFrustum is called.
			FrustumVertices = new[]
			{
				new VertexPositionColor(Vector3.Zero, color), // 0 = Back  Left  Bottom
				new VertexPositionColor(Vector3.Zero, color), // 1 = Back  Right Bottom
				new VertexPositionColor(Vector3.Zero, color), // 2 = Back  Left  Top
				new VertexPositionColor(Vector3.Zero, color), // 3 = Back  Right Top
				new VertexPositionColor(Vector3.Zero, color), // 4 = Front Left  Bottom
				new VertexPositionColor(Vector3.Zero, color), // 5 = Front Right Bottom
				new VertexPositionColor(Vector3.Zero, color), // 6 = Front Left  Top
				new VertexPositionColor(Vector3.Zero, color)  // 7 = Front Right Top
			};

			WireFrustumIndices = new short[]
			{
				0, 1,
				2, 3,
				4, 5,
				6, 7,
				4, 0,
				5, 1,
				6, 2,
				7, 3,
				0, 2,
				1, 3,
				4, 6,
				5, 7
			};

			Corners = new Vector3[8];
		}

		public static void DrawWireframe(PrimitiveDrawer primitiveDrawer,
			Matrix cameraView, Matrix cameraProjection,
			BoundingFrustum frustum, Color color)
		{
			// The points returned correspond to the corners of the BoundingFrustum faces that are 
			// perpendicular to the z-axis. The near face is the face with the larger z value, and 
			// the far face is the face with the smaller z value. Points 0 to 3 correspond to the 
			// near face in a clockwise order starting at its upper-left corner when looking toward 
			// the origin from the positive z direction. Points 4 to 7 correspond to the far face 
			// in a clockwise order starting at its upper-left corner when looking toward the 
			// origin from the positive z direction.
			frustum.GetCorners(Corners);

			FrustumVertices[6].Position = Corners[0];
			FrustumVertices[7].Position = Corners[1];
			FrustumVertices[5].Position = Corners[2];
			FrustumVertices[4].Position = Corners[3];
			FrustumVertices[2].Position = Corners[4];
			FrustumVertices[3].Position = Corners[5];
			FrustumVertices[1].Position = Corners[6];
			FrustumVertices[0].Position = Corners[7];

			primitiveDrawer.Draw(Matrix.Identity, cameraView, cameraProjection, color, null,
				PrimitiveType.LineList, FrustumVertices, WireFrustumIndices, false);
		}
	}
}