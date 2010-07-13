using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Osiris.Graphics.Shaders;
using System.Collections.Generic;

namespace Osiris.Content.Pipeline.Graphics.Shaders
{
	public class CompiledShaderFragmentContent
	{
		public List<string> EffectParameters;
		public string MangledNamePrefix;
		public string Name;
	}
}