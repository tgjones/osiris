using System;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Graphics.Shadows
{
	public interface IShadowMapService
	{
		Texture2D[] ShadowMapTextures
		{
			get;
		}

		int ShadowMapSize
		{
			get;
		}
	}
}
