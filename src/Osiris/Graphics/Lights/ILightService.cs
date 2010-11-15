using System;
using Microsoft.Xna.Framework;

namespace Osiris.Graphics.Lights
{
	public interface ILightService : IProjector
	{
		Vector4 Diffuse
		{
			get;
		}

		Vector4 Ambient
		{
			get;
		}

		float Angle
		{
			get;
			set;
		}
	}
}
