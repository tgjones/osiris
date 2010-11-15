using System;
using Microsoft.Xna.Framework;

namespace Osiris.Graphics
{
	public interface IProjector
	{
		Matrix ViewProjectionMatrix
		{
			get;
		}

		Matrix ViewMatrix
		{
			get;
		}

		Matrix ProjectionMatrix
		{
			get;
		}

		float ProjectionNear
		{
			get;
		}

		float ProjectionFar
		{
			get;
		}

		Vector3 Direction
		{
			get;
		}
	}
}
