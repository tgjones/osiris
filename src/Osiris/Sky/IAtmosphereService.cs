using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Osiris.Graphics;

namespace Osiris.Sky
{
	public interface IAtmosphereService
	{
		float EarthRadius
		{
			get;
		}

		float AtmosphereDepth
		{
			get;
		}

		float SkydomeRadius
		{
			get;
		}

		void SetEffectParameters(ExtendedEffect effect, bool fudgePosition);
	}
}
