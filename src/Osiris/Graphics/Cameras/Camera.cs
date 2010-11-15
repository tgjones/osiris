using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Graphics.Cameras
{
	public abstract class Camera : Projector, ICameraService, IViewer
	{
		#region Fields

		protected BoundingFrustum _boundingFrustum;

		#endregion

		#region Properties

		public BoundingFrustum BoundingFrustum
		{
			get
			{
				if (_boundingFrustum == null)
					_boundingFrustum = new BoundingFrustum(this.ViewProjectionMatrix);
				return _boundingFrustum;
			}
		}

		public abstract Vector3 Position
		{
			get;
		}

		public Vector2 Position2D
		{
			get { return new Vector2(Position.X, Position.Z); }
		}

		public float ProjectionTop
		{
			get { return 0.5f; }
		}

		#endregion

		#region Constructor

		public Camera(Game game)
			: base(game)
		{
			game.Services.AddService(typeof(ICameraService), this);
			game.Components.ComponentRemoved += new EventHandler<GameComponentCollectionEventArgs>(Components_ComponentRemoved);

			GraphicsDevice graphicsDevice = ((IGraphicsDeviceService) game.Services.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;
			float aspectRatio = (float) graphicsDevice.Viewport.Width / (float) graphicsDevice.Viewport.Height;
			float projectionLeft = -0.5f * aspectRatio;
			float projectionRight = 0.5f * aspectRatio;
			float projectionBottom = -0.5f;
			_near = 1;
			_far = 3000000;

			_projection = Matrix.CreatePerspectiveOffCenter(
				projectionLeft,
				projectionRight,
				projectionBottom,
				this.ProjectionTop,
				_near,
				_far);

			_boundingFrustum = null;
		}

		private void Components_ComponentRemoved(object sender, GameComponentCollectionEventArgs e)
		{
			if (e.GameComponent == this)
				Game.Services.RemoveService(typeof(ICameraService));
		}

		#endregion
	}
}
