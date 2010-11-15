using System;
using Microsoft.Xna.Framework;

namespace Osiris.Graphics.Cameras
{
	public interface ICameraService : IProjector
	{
		BoundingFrustum BoundingFrustum
		{
			get;
		}

		Vector3 Position
		{
			get;
		}

		float ProjectionTop
		{
			get;
		}

		void Update(GameTime gameTime);
	}
}
