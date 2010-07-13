using System;

namespace Osiris.Graphics.Rendering.Lights
{
	public class AmbientLight : Light
	{
		public override LightType Type
		{
			get { return LightType.Ambient; }
		}
	}
}
