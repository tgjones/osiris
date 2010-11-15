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
			private set;
		}

		public CompiledShaderFragment[] CompiledFragments
		{
			get;
			private set;
		}

		public VertexElement[] VertexElements
		{
			get;
			private set;
		}

		public Shader(Effect effect, Dictionary<string, EffectParameter> rendererConstants, CompiledShaderFragment[] compiledFragments, VertexElement[] vertexElements)
		{
			Effect = effect;
			RendererConstants = rendererConstants;
			CompiledFragments = compiledFragments;
			VertexElements = vertexElements;
		}

		public void SetParameterValues()
		{
			foreach (CompiledShaderFragment compiledFragment in CompiledFragments)
				compiledFragment.SetParameterValues();
		}
	}
}
