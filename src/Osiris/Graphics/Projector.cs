using System;
using Microsoft.Xna.Framework;

namespace Osiris.Graphics
{
	public abstract class Projector : Microsoft.Xna.Framework.GameComponent, IProjector
	{
		#region Fields

		protected Matrix _projection;
		protected Matrix _view;

		protected float _near;
		protected float _far;

		#endregion

		#region Properties

		public Matrix ViewProjectionMatrix
		{
			get { return _view * _projection; }
		}

		public Matrix ViewMatrix
		{
			get { return _view; }
		}

		public Matrix ProjectionMatrix
		{
			get { return _projection; }
		}

		public float ProjectionNear
		{
			get { return _near; }
		}

		public float ProjectionFar
		{
			get { return _far; }
		}

		public abstract Vector3 Direction
		{
			get;
		}

		#endregion

		#region Constructor

		public Projector(Game game)
			: base(game)
		{
			this.UpdateOrder = 1000;
		}

		#endregion
	}
}
