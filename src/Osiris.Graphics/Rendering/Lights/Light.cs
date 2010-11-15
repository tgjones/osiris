using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Osiris.Graphics.Rendering
{
	public abstract class Light
	{
		public abstract LightType Type
		{
			get;
		}

		public Color Ambient;
		public Color Intensity;

		public enum LightType
		{
			Ambient,
			Directional,
			Point,
			Spot
		}
	}
}
