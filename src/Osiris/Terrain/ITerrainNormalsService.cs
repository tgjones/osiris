using System;
using Microsoft.Xna.Framework;

namespace Osiris.Terrain
{
	public interface ITerrainNormalsService
	{
		Vector3 this[int x, int z]
		{
			get;
		}
	}
}
