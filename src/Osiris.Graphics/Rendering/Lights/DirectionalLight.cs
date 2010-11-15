using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Graphics.Rendering
{
	public class DirectionalLight : Light
	{
		public override LightType Type
		{
			get { return LightType.Directional; }
		}

		public Vector3 Direction;
		public Color Diffuse;
		public Color Specular;
	}
}
