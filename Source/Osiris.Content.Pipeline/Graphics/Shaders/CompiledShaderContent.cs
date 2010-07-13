using System;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Osiris.Content.Pipeline.Graphics.Shaders
{
	public class CompiledShaderContent
	{
		public CompiledEffect CompiledEffect;
		public Dictionary<string, string> RendererConstants;
		public List<CompiledShaderFragmentContent> CompiledShaderFragments;
		public VertexElement[] VertexElements;
	}
}
