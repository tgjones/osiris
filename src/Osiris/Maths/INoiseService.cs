using System;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Maths
{
	public interface INoiseService
	{
		Texture2D GetPermutationTexture();
		Texture2D GetGradientTexture();
	}
}
