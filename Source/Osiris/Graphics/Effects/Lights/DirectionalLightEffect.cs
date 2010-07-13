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
		public Vector3 Direction
		{
			get;
			set;
		}

		public Color DiffuseColour
		{
			get;
			set;
		}

		public DirectionalLightEffect(IServiceProvider serviceProvider)
			: base(serviceProvider)
		{

		}

		public override string GetShaderFragmentName()
		{
			return "Osiris.Lights.DirectionalLight";
		}

		public override void SetParameterValues(CompiledShaderFragment compiledShaderFragment)
		{
			compiledShaderFragment.GetParameter("Direction").SetValue(Direction);
			compiledShaderFragment.GetParameter("DiffuseColour").SetValue(DiffuseColour.ToVector4());
		}
	}
}
