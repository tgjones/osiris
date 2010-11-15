using System;
using Osiris.Graphics.Shaders;
using Osiris.Graphics.Rendering;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics.SceneGraph;

namespace Osiris.Graphics.Effects
{
	public class PixelColourOutputEffect : ShaderEffect
	{
		public PixelColourOutputEffect(IServiceProvider serviceProvider)
			: base(serviceProvider)
		{

		}

		protected override string GetShaderFragmentName()
		{
			return @"ShaderFragments\PixelColourOutput";
		}
	}
}
