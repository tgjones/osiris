using System;
using Osiris.Graphics.Shaders;
using Osiris.Graphics.Rendering;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics.SceneGraph;

namespace Osiris.Graphics.Effects.Lights
{
	public class DirectionalLightEffect : ShaderEffect
	{
		private CompiledShaderFragment _compiledShaderFragment;

		public Color AmbientLightDiffuseColour
		{
			get;
			set;
		}

		public Vector3 LightDirection
		{
			get;
			set;
		}

		public Color LightDiffuseColour
		{
			get;
			set;
		}

		public DirectionalLightEffect(IServiceProvider serviceProvider)
			: base(serviceProvider)
		{

		}

		protected override string GetShaderFragmentName()
		{
			return @"ShaderFragments\Lights\DirectionalLight";
		}

		public override void SetParameterValues(CompiledShaderFragment compiledShaderFragment)
		{
			compiledShaderFragment.GetParameter("AmbientLightDiffuseColour").SetValue(AmbientLightDiffuseColour.ToVector4());
			compiledShaderFragment.GetParameter("LightDirection").SetValue(LightDirection);
			compiledShaderFragment.GetParameter("LightDiffuseColour").SetValue(LightDiffuseColour.ToVector4());
		}
	}
}
