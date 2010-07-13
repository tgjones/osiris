using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Graphics.Rendering.Cameras
{
	public class Camera
	{
		public event EventHandler<CameraEventArgs> Moved;

		private Vector3 _position;

		public Viewport Viewport
		{
			get;
			protected set;
		}

		public Matrix ViewMatrix
		{
			get;
			protected set;
		}

		public Matrix ProjectionMatrix
		{
			get;
			protected set;
		}

		public Matrix ViewProjectionMatrix
		{
			get { return ViewMatrix * ProjectionMatrix; }
		}

		public BoundingFrustum Frustum
		{
			get { return new BoundingFrustum(ViewProjectionMatrix); }
		}

		public Vector3 Position
		{
			get { return _position; }
			set
			{
				_position = value;
				if (Moved != null)
					Moved(this, new CameraEventArgs { Camera = this });
			}
		}
	}

	public class CameraEventArgs : EventArgs
	{
		public Camera Camera
		{
			get;
			set;
		}
	}
}
