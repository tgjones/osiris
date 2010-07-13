using System.Collections.Generic;
using Osiris.Graphics.Rendering;
using Osiris.Graphics.Shaders;
using System;
using Microsoft.Xna.Framework.Content;
using Osiris.Graphics.SceneGraph;

namespace Osiris.Graphics.Effects
{
	public abstract class ShaderEffect
	{
		protected IServiceProvider ServiceProvider
		{
			get;
			private set;
		}

		protected ShaderEffect(IServiceProvider serviceProvider)
		{
			ServiceProvider = serviceProvider;
		}

		public abstract string GetShaderFragmentName();

		public virtual void SetParameterValues(CompiledShaderFragment compiledShaderFragment) { }

		/// <summary>
		/// The derived effect class uses this method for two purposes:
		/// (1) Performing any pre-process steps. For example, an environment map
		/// effect would render the six faces of a cube map here.
		/// (2) Setting effect parameters for the main draw pass. For example,
		/// an environment map would set the generated cube map texture into
		/// the relevant effect parameter.
		/// </summary>
		public virtual void DrawPreProcess(Spatial spatial) { }
	}
}
