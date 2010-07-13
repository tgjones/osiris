using System;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Graphics.Rendering.GlobalStates
{
	public class DepthBufferState : GlobalState
	{
		public bool Enabled;
		public bool Writable;
		public CompareFunction Compare;

		public override StateType Type
		{
			get { return StateType.DepthBuffer; }
		}

		static DepthBufferState()
		{
			GlobalStateCollection.Default[StateType.DepthBuffer] = new DepthBufferState();
		}

		public DepthBufferState()
		{
			Enabled = true;
			Writable = true;
			Compare = CompareFunction.LessEqual;
		}
	}
}
