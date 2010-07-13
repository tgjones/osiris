using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics.Effects;

namespace Osiris.Graphics.Shaders
{
	public class CompiledShaderFragment
	{
		private ShaderEffect _shaderEffect;
		private Dictionary<string, EffectParameter> _effectParameters;
		private string _mangledNamePrefix;

		//public ShaderFragment ShaderFragment { get; private set; }

		public CompiledShaderFragment(ShaderEffect shaderEffect, List<EffectParameter> effectParameters, string mangledNamePrefix)
		{
			_shaderEffect = shaderEffect;

			_effectParameters = new Dictionary<string, EffectParameter>();
			effectParameters.ForEach(p => _effectParameters.Add(p.Name, p));

			_mangledNamePrefix = mangledNamePrefix;
		}

		public EffectParameter GetParameter(string name)
		{
			return _effectParameters[_mangledNamePrefix + name];
		}

		public void SetParameterValues()
		{
			_shaderEffect.SetParameterValues(this);
		}
	}
}
