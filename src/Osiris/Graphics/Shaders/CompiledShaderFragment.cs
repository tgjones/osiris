using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics.Effects;

namespace Osiris.Graphics.Shaders
{
	public class CompiledShaderFragment
	{
		private ShaderEffect _shaderEffect;

		internal Dictionary<string, EffectParameter> EffectParameters
		{
			get;
			private set;
		}

		internal string MangledNamePrefix
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			private set;
		}

		public CompiledShaderFragment(ShaderEffect shaderEffect, List<EffectParameter> effectParameters, string mangledNamePrefix, string name)
		{
			_shaderEffect = shaderEffect;

			EffectParameters = new Dictionary<string, EffectParameter>();
			effectParameters.ForEach(p => EffectParameters.Add(p.Name, p));

			MangledNamePrefix = mangledNamePrefix;
			Name = name;
		}

		public EffectParameter GetParameter(string name)
		{
			return EffectParameters[MangledNamePrefix + name];
		}

		public void SetParameterValues()
		{
			// There might not be an attached effect if there are no parameter values.
			if (_shaderEffect != null)
				_shaderEffect.SetParameterValues(this);
		}
	}
}
