using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Diagnostics.Shapes
{
	internal static class Disc
	{
		public static void DrawWireframe(
			PrimitiveDrawer primitiveDrawer, Vector3 cameraPosition,
			Matrix cameraView, Matrix cameraProjection,
			Vector3 center, Vector3 normal, float radius,
			Color color, bool fadeBackFace)
		{
			// Calculate basis vectors.
			Vector3 from = Vector3.Cross(normal, Vector3.Up);
			if (from.LengthSquared() < 0.0001f)
				from = Vector3.Cross(normal, Vector3.Right);
			from.Normalize();

			// We need two vertices per line, so we can allocate our vertices.
			const int numSegments = 64;
			const int numLines = numSegments + 1;
			var vertices = new VertexPositionColor[numLines * 2];

			// Calculate initial orientation.
			Quaternion rotation = Quaternion.CreateFromAxisAngle(normal, MathHelper.TwoPi / (numSegments - 1));

			// Compute vertex positions.
			Vector3 edge = from * radius;
			for (int i = 0; i < numSegments; i++)
			{
				// Calculate line positions.
				Vector3 start = center + edge;
				edge = Vector3.Transform(edge, rotation);
				Vector3 end = center + edge;

				// Calculate line normal.
				Vector3 cameraToEdge = start - cameraPosition;
				var lineColor = color;
				if (fadeBackFace && Vector3.Dot(cameraToEdge, edge) > 0)
					lineColor = Color.FromNonPremultiplied(color.R, color.G, color.B, 25);

				vertices[(i * 2) + 0] = new VertexPositionColor(start, lineColor);
				vertices[(i * 2) + 1] = new VertexPositionColor(end, lineColor);
			}

			primitiveDrawer.Draw(Matrix.Identity, cameraView, cameraProjection,
				Color.White, null, PrimitiveType.LineList, vertices, false);
		}
	}
}