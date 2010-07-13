using System;

namespace Osiris.Graphics.Rendering.GlobalStates
{
	public abstract class GlobalState
	{
		public abstract StateType Type
		{
			get;
		}

		/// <summary>
		/// Supported global states.
		/// </summary>
		public enum StateType
		{
			Alpha,
			Cull,
			PolygonOffset,
			Stencil,
			Wireframe,
			DepthBuffer,
			MAX
		}
	}
}
