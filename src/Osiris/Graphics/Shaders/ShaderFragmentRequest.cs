using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Osiris.Graphics.Effects;

namespace Osiris.Graphics.Shaders
{
	public class ShaderFragmentRequest
	{
		public string Name;

		/// <summary>
		/// Can be null
		/// </summary>
		public ShaderEffect ShaderEffect;
	}
}
