using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Diagnostics.Shapes;
using Rectangle = Osiris.Diagnostics.Shapes.Rectangle;

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
				if (_primitiveDrawer != null)
					_primitiveDrawer.Dispose();
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
			Box.DrawWireframe(_primitiveDrawer, cameraPosition, cameraView, cameraProjection,
				center, size, rotation, color);
		}

		public static void DrawWireframeFrustum(Matrix cameraView, Matrix cameraProjection, BoundingFrustum frustum, Color color)
		{
			EnsureGraphicsDevice();
			Frustum.DrawWireframe(_primitiveDrawer, cameraView, cameraProjection, frustum, color);
		}

		public static void DrawSolidRectangle(Matrix cameraView, Matrix cameraProjection, Vector3[] corners, Color color)
		{
			EnsureGraphicsDevice();
			Rectangle.DrawSolid(_primitiveDrawer, cameraView, cameraProjection, corners, color);
		}

		public static void DrawLine(Matrix cameraView, Matrix cameraProjection, Vector3 start, Vector3 end, Color color)
		{
			EnsureGraphicsDevice();
			Line.Draw(_primitiveDrawer, cameraView, cameraProjection, start, end, color);
		}

		public static void DrawWireframeDisc(Vector3 cameraPosition, Matrix cameraView, Matrix cameraProjection,
			Vector3 center, Vector3 normal, float radius, Color color, bool fadeBackFace)
		{
			EnsureGraphicsDevice();
			Disc.DrawWireframe(_primitiveDrawer, cameraPosition, cameraView, cameraProjection, center, normal, radius, color, fadeBackFace);
		}

		public static void DrawWireframeSphere(Vector3 cameraPosition, Matrix cameraView, Matrix cameraProjection,
			Vector3 center, Vector3 normal, float radius, Quaternion rotation, Color color)
		{
			EnsureGraphicsDevice();
			Sphere.DrawWireframe(_primitiveDrawer, cameraPosition, cameraView, cameraProjection, center, radius, rotation, color);
		}
	}
}