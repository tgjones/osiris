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
			Vector3 from = Vector3.Cross(normal, Vector3.Up);
			if (from.LengthSquared() < 0.0001f)
				from = Vector3.Cross(normal, Vector3.Right);

			from.Normalize();

			Vector3 cameraToCenter = center - cameraPosition;

			const int numSegments = 64;
			Quaternion quaternion = Quaternion.CreateFromAxisAngle(normal, MathHelper.TwoPi / (numSegments - 1));
			var vertices = new VertexPositionColor[numSegments * 2];
			Vector3 edge = from * radius;
			for (int i = 0; i < numSegments; i++)
			{
				var lineColor = color;
				if (fadeBackFace && Vector3.Dot(cameraToCenter, edge) > 0)
					lineColor = Color.FromNonPremultiplied(color.R, color.G, color.B, 25);

				vertices[i * 2] = new VertexPositionColor(center + edge, lineColor);
				edge = Vector3.Transform(edge, quaternion);
				vertices[(i * 2) + 1] = new VertexPositionColor(center + edge, lineColor);
			}

			primitiveDrawer.Draw(Matrix.Identity, cameraView, cameraProjection,
				Color.White, null, PrimitiveType.LineList, vertices, false);
		}
	}
}