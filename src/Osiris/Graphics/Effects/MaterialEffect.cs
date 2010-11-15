using System;
using Osiris.Graphics.Shaders;
using Osiris.Graphics.Rendering;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics.SceneGraph;

namespace Osiris.Graphics.Effects
{
	public class MaterialEffect : ShaderEffect
	{
		public float Alpha
		{
			get;
			set;
		}

		public Color DiffuseColour
		{
			get;
			set;
		}

		public Color SpecularColour
		{
			get;
			set;
		}

		public float SpecularPower
		{
			get;
			set;
		}

		public float SpecularIntensity
		{
			get;
			set;
		}

		public float Roughness
		{
			get;
			set;
		}

		public bool TextureEnabled
		{
			get;
			set;
		}

		public Texture2D DiffuseTexture
		{
			get;
			set;
		}

		public MaterialEffect(IServiceProvider serviceProvider)
			: base(serviceProvider)
		{

		}

		public override string GetShaderFragmentName()
		{
			return "Osiris.BasicMaterial";
		}

		public override void SetParameterValues(CompiledShaderFragment compiledFragment)
		{
			compiledFragment.GetParameter("Alpha").SetValue(Alpha);
			compiledFragment.GetParameter("DiffuseColour").SetValue(DiffuseColour.ToVector3());
			compiledFragment.GetParameter("SpecularColour").SetValue(SpecularColour.ToVector3());
			compiledFragment.GetParameter("SpecularPower").SetValue(SpecularPower);
			compiledFragment.GetParameter("SpecularIntensity").SetValue(SpecularIntensity);
			compiledFragment.GetParameter("Roughness").SetValue(Roughness);
			compiledFragment.GetParameter("TextureEnabled").SetValue(TextureEnabled);
			compiledFragment.GetParameter("DiffuseTexture").SetValue(DiffuseTexture);
		}
	}
}
