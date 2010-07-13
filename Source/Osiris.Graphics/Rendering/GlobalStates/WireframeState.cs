using System;

namespace Osiris.Graphics.Rendering.GlobalStates
{
	public class WireframeState : GlobalState
	{
		public bool Enabled;

		public override StateType Type
		{
			get { return StateType.Wireframe; }
		}

		static WireframeState()
		{
			GlobalStateCollection.Default[StateType.Wireframe] = new WireframeState();
		}

		public WireframeState()
		{
			Enabled = false;
		}
	}
}
