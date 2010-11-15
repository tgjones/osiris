using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Graphics.Shaders
{
	public class CompiledShaderFragment
	{
		private Shader _shader;
		private string _mangledNamePrefix;

		public ShaderFragment ShaderFragment { get; private set; }

		public CompiledShaderFragment(ShaderEffect shaderEffect, EffectParameter[] effectParameters)
		{
			_shader = shader;
			ShaderFragment = shaderFragment;
			_mangledNamePrefix = mangledNamePrefix;
		}

		public EffectParameter GetParameter(string name)
		{
			return _shader.Effect.Parameters[_mangledNamePrefix + name];
		}
	}
}
