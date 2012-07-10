using Microsoft.Xna.Framework;

namespace Osiris.Terrain.Graphics
{
	public interface ICameraService
	{
		Matrix Projection { get; }
		Matrix View { get; }

		float ProjectionNear
		{
			get;
		}

		float ProjectionTop
		{
			get;
		}

		Vector3 Position
		{
			get;
		}
	}
}