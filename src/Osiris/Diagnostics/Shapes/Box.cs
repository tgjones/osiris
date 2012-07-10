using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Diagnostics.Shapes
{
	internal static class Box
	{
		private static readonly Vector3[] WireframeCorners;
		private static readonly WireframeLine[] WireframeLines;

		static Box()
		{
			WireframeCorners = new[]
			{
				new Vector3(-0.5f, -0.5f, -0.5f), // 0 = Back  Left  Bottom
				new Vector3(+0.5f, -0.5f, -0.5f), // 1 = Back  Right Bottom
				new Vector3(-0.5f, +0.5f, -0.5f), // 2 = Back  Left  Top
				new Vector3(+0.5f, +0.5f, -0.5f), // 3 = Back  Right Top
				new Vector3(-0.5f, -0.5f, +0.5f), // 4 = Front Left  Bottom
				new Vector3(+0.5f, -0.5f, +0.5f), // 5 = Front Right Bottom
				new Vector3(-0.5f, +0.5f, +0.5f), // 6 = Front Left  Top
				new Vector3(+0.5f, +0.5f, +0.5f)  // 7 = Front Right Top
			};

			WireframeLines = new[]
			{
				new WireframeLine(WireframeCorners[0], WireframeCorners[1], Vector3.Forward, Vector3.Down),  // Back  Bottom
				new WireframeLine(WireframeCorners[2], WireframeCorners[3], Vector3.Forward, Vector3.Up),    // Back  Top
				new WireframeLine(WireframeCorners[4], WireframeCorners[5], Vector3.Backward, Vector3.Down), // Front Bottom
				new WireframeLine(WireframeCorners[6], WireframeCorners[7], Vector3.Backward, Vector3.Up),   // Front Top

				new WireframeLine(WireframeCorners[0], WireframeCorners[4], Vector3.Left, Vector3.Down),     // Left  Bottom
				new WireframeLine(WireframeCorners[2], WireframeCorners[6], Vector3.Left, Vector3.Up),       // Left  Top
				new WireframeLine(WireframeCorners[1], WireframeCorners[5], Vector3.Right, Vector3.Down),    // Right Bottom
				new WireframeLine(WireframeCorners[3], WireframeCorners[7], Vector3.Right, Vector3.Up),      // Right Top

				new WireframeLine(WireframeCorners[0], WireframeCorners[2], Vector3.Forward, Vector3.Left),  // Back  Left
				new WireframeLine(WireframeCorners[1], WireframeCorners[3], Vector3.Forward, Vector3.Right), // Back  Right
				new WireframeLine(WireframeCorners[4], WireframeCorners[6], Vector3.Backward, Vector3.Left), // Front Left
				new WireframeLine(WireframeCorners[5], WireframeCorners[7], Vector3.Backward, Vector3.Right) // Front Right
			};
		}

		private struct WireframeLine
		{
			public readonly Vector3 Point1;
			public readonly Vector3 Point2;
			public readonly Vector3 FaceNormal1;
			public readonly Vector3 FaceNormal2;

			public WireframeLine(Vector3 point1, Vector3 point2, Vector3 faceNormal1, Vector3 faceNormal2)
			{
				Point1 = point1;
				Point2 = point2;
				FaceNormal1 = faceNormal1;
				FaceNormal2 = faceNormal2;
			}

			public static WireframeLine Transform(WireframeLine line, Matrix matrix)
			{
				return new WireframeLine(
					Vector3.Transform(line.Point1, matrix),
					Vector3.Transform(line.Point2, matrix),
					Vector3.Normalize(Vector3.TransformNormal(line.FaceNormal1, matrix)),
					Vector3.Normalize(Vector3.TransformNormal(line.FaceNormal2, matrix)));
			}
		}

		public static void DrawWireframe(PrimitiveDrawer primitiveDrawer, Vector3 cameraPosition, Matrix cameraView, Matrix cameraProjection,
			Vector3 center, Vector3 size, Quaternion rotation, Color color)
		{
			var world = Matrix.CreateScale(size) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(center);

			var vertices = new VertexPositionColor[WireframeLines.Length * 2];
			for (int i = 0; i < WireframeLines.Length; i++)
			{
				var transformedLine = WireframeLine.Transform(WireframeLines[i], world);
				Vector3 cameraToLine = Vector3.Normalize(((transformedLine.Point1 + transformedLine.Point2) / 2) - cameraPosition);

				Color lineColor = color;
				if (!IsCameraFacing(cameraToLine, transformedLine.FaceNormal1) && !IsCameraFacing(cameraToLine, transformedLine.FaceNormal2))
					lineColor = Color.FromNonPremultiplied(color.R, color.G, color.B, 25);

				vertices[i * 2] = new VertexPositionColor(transformedLine.Point1, lineColor);
				vertices[(i * 2) + 1] = new VertexPositionColor(transformedLine.Point2, lineColor);
			}

			primitiveDrawer.Draw(Matrix.Identity, cameraView, cameraProjection,
				Color.White, null, PrimitiveType.LineList, vertices, false);
		}

		private static bool IsCameraFacing(Vector3 cameraToLine, Vector3 faceNormal)
		{
			return Vector3.Dot(cameraToLine, faceNormal) <= 0;
		}
	}
}