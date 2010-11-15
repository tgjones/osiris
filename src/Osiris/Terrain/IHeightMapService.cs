using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Terrain
{
	public interface IHeightMapService
	{
		int Width
		{
			get;
		}

		int Height
		{
			get;
		}

		float this[int x, int z]
		{
			get;
		}

		float this[float x, float z]
		{
			get;
		}

		Vector3 GetPosition(int x, int z);

		Texture2D GetTexture();
	}
}
