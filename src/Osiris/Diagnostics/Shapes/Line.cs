using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Diagnostics.Shapes
{
	internal static class Line
	{
		public static void Draw(
			PrimitiveDrawer primitiveDrawer, Matrix cameraView, Matrix cameraProjection,
			Vector3 start, Vector3 end, Color color)
		{
			var vertices = new[]
			{
				new VertexPositionColor(start, Color.White),
				new VertexPositionColor(end, Color.White)
			};
			primitiveDrawer.Draw(Matrix.Identity, cameraView, cameraProjection,
				color, null, PrimitiveType.LineList, vertices, false);
		}
	}
}