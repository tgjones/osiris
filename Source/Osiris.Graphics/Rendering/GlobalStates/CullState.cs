using System;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Graphics.Rendering.GlobalStates
{
	public class CullState : GlobalState
	{
		public bool Enabled;
		public CullMode CullFace;

		public override StateType Type
		{
			get { return StateType.Cull; }
		}

		static CullState()
		{
			GlobalStateCollection.Default[StateType.Cull] = new CullState();
		}

		public CullState()
		{
			Enabled = true;
			CullFace = CullMode.CullClockwiseFace;
		}
	}
}
