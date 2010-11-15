using System;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Osiris.Graphics.Shaders
{
	public class Shader
	{
		public Effect Effect
		{
			get;
			private set;
		}

		public Dictionary<string, EffectParameter> RendererConstants
		{
			get;
			set;
		}

		public CompiledShaderFragment[] CompiledFragments
		{
			get;
			private set;
		}

		public Shader(GraphicsDevice graphicsDevice, CompiledEffect compiledEffect, CompiledShaderFragment[] compiledFragments)
		{
			Effect = new Effect(graphicsDevice, compiledEffect.GetEffectCode(), CompilerOptions.Debug, null);
			CompiledFragments = compiledFragments;
		}
	}
}
