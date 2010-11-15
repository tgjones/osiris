using System;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Graphics.Rendering.GlobalStates
{
	public class StencilState : GlobalState
	{
		public bool Enabled;
		public CompareFunction Compare;
		public int Reference;
		public int Mask;
		public int WriteMask;
		public StencilOperation OnFail;
		public StencilOperation OnDepthBufferFail;
		public StencilOperation OnDepthBufferPass;

		public override StateType Type
		{
			get { return StateType.Stencil; }
		}

		static StencilState()
		{
			GlobalStateCollection.Default[StateType.Stencil] = new StencilState();
		}

		public StencilState()
		{
			Enabled = false;
			Compare = CompareFunction.Never;
			Reference = 0;
			Mask = int.MaxValue;
			WriteMask = int.MaxValue;
			OnFail = StencilOperation.Keep;
			OnDepthBufferFail = StencilOperation.Keep;
			OnDepthBufferPass = StencilOperation.Keep;
		}
	}
}
