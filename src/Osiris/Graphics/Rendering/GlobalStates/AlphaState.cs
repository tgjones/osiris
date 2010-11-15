using System;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Graphics.Rendering.GlobalStates
{
	public class AlphaState : GlobalState
	{
		public bool BlendEnabled;
		public Blend SourceBlend;
		public Blend DestinationBlend;

		public bool TestEnabled;
		public CompareFunction Test;
		public int Reference;

		public override StateType Type
		{
			get { return StateType.Alpha; }
		}

		static AlphaState()
		{
			GlobalStateCollection.Default[StateType.Alpha] = new AlphaState();
		}

		public AlphaState()
		{
			BlendEnabled = false;
			SourceBlend = Blend.SourceAlpha;
			DestinationBlend = Blend.InverseSourceAlpha;

			TestEnabled = false;
			Test = CompareFunction.Always;
			Reference = 0;
		}
	}
}
