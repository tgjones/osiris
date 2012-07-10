using System;
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

		private static void EnsureGraphicsDevice()
		{
			if (_graphicsDevice == null)
				throw new InvalidOperationException("ShapeVisualizer.GraphicsDevice must be set before calling any Draw* methods.");
		}

		public static void DrawWireframeBox(Vector3 cameraPosition, Matrix cameraView, Matrix cameraProjection,
			Vector3 center, Vector3 size, Quaternion rotation, Color color)
		{
			EnsureGraphicsDevice();
			BoxVisualizer.DrawWireframe(_primitiveDrawer, cameraPosition, cameraView, cameraProjection,
				center, size, rotation, color);
		}
	}
}