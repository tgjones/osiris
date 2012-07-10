using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Diagnostics
{
	public static class ShapeVisualizer
	{
		private static GraphicsDevice _graphicsDevice;
		private static PrimitiveDrawer _primitiveDrawer;

		public static GraphicsDevice GraphicsDevice
		{
			get { return _graphicsDevice; }
			set
			{
				_graphicsDevice = value;
				_primitiveDrawer = new PrimitiveDrawer(value);
			}
		}

		public static void DrawWireframeBox(Vector3 cameraPosition, Matrix cameraView, Matrix cameraProjection,
			Vector3 center, Vector3 size, Quaternion rotation, Color color)
		{
			BoxVisualizer.DrawWireframe(_primitiveDrawer, cameraPosition, cameraView, cameraProjection,
				center, size, rotation, color);
		}
	}
}