using Microsoft.Xna.Framework;

namespace Osiris.Diagnostics.Shapes
{
	internal static class Sphere
	{
		public static void DrawWireframe(PrimitiveDrawer primitiveDrawer,
			Vector3 cameraPosition, Matrix cameraView, Matrix cameraProjection,
			Vector3 center, float radius, Quaternion rotation, Color color)
		{
			// Three discs
			Disc.DrawWireframe(primitiveDrawer, cameraPosition, cameraView, cameraProjection, center, Vector3.Transform(Vector3.Up, rotation), radius, color, true);
			Disc.DrawWireframe(primitiveDrawer, cameraPosition, cameraView, cameraProjection, center, Vector3.Transform(Vector3.Forward, rotation), radius, color, true);
			Disc.DrawWireframe(primitiveDrawer, cameraPosition, cameraView, cameraProjection, center, Vector3.Transform(Vector3.Left, rotation), radius, color, true);

			// Disc aligned with camera
			Disc.DrawWireframe(primitiveDrawer, cameraPosition, cameraView, cameraProjection, center,
				Vector3.Normalize(center - cameraPosition), radius * 1.1f, Color.Blue, false);
		}
	}
}