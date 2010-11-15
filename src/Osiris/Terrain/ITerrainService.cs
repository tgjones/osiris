using System;
using Microsoft.Xna.Framework;

namespace Osiris.Terrain
{
	public interface ITerrainService
	{
		void GetHeightAndNormalAtPoints(Vector2[] p, out float[] height, out Vector3[] normal);
	}
}
