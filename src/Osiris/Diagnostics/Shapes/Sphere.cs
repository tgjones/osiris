using Microsoft.Xna.Framework;

namespace Osiris.Diagnostics.Shapes
{
	internal static class Sphere
	{
		public static void DrawWireframe(PrimitiveDrawer primitiveDrawer,
			Vector3 cameraPosition, Matrix cameraView, Matrix cameraProjection,
			Vector3 center, float radius, Quaternion rotation, Color color)
		{
			// Draw three orthogonal discs.
			Disc.DrawWireframe(primitiveDrawer, cameraPosition, cameraView, cameraProjection, center, Vector3.Normalize(Vector3.Transform(Vector3.Up, rotation)), radius, color, true);
			Disc.DrawWireframe(primitiveDrawer, cameraPosition, cameraView, cameraProjection, center, Vector3.Normalize(Vector3.Transform(Vector3.Forward, rotation)), radius, color, true);
			Disc.DrawWireframe(primitiveDrawer, cameraPosition, cameraView, cameraProjection, center, Vector3.Normalize(Vector3.Transform(Vector3.Left, rotation)), radius, color, true);

			// Draw disc aligned with camera. To do this, first calculate the largest visible cross section using
 			// the technique described here: http://www.quantimegroup.com/solutions/pages/Article/Article.html

			// Solve for dy.
			Vector3 cameraToCenter = center - cameraPosition;
			float distanceToCenter = cameraToCenter.Length();
			float radius2 = radius * radius;
			float dy = radius2 / distanceToCenter;
			float r = MathUtility.Sqrt(radius2 - (dy * dy));

			Vector3 directionToCenter = Vector3.Normalize(cameraToCenter);
			Vector3 newCenter = cameraPosition + directionToCenter * (distanceToCenter - dy);

			// Disc aligned with camera
			Disc.DrawWireframe(primitiveDrawer, cameraPosition, cameraView, cameraProjection,
				newCenter, directionToCenter, r, Color.White, false);
		}
	}
}