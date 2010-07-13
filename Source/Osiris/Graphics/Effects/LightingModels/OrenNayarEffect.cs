using System;
using Osiris.Graphics.Shaders;
using Osiris.Graphics.Rendering;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics.SceneGraph;

namespace Osiris.Graphics.Effects.LightingModels
{
	public class OrenNayarEffect : ShaderEffect
	{
		public Color AmbientLightDiffuseColour
		{
			get;
			set;
		}

		public OrenNayarEffect(IServiceProvider serviceProvider)
			: base(serviceProvider)
		{

		}

		public override string GetShaderFragmentName()
		{
			return "Osiris.LightingModels.OrenNayar";
		}

		public override void SetParameterValues(CompiledShaderFragment compiledShaderFragment)
		{
			compiledShaderFragment.GetParameter("AmbientLightDiffuseColour").SetValue(AmbientLightDiffuseColour.ToVector4());
		}
	}
}
