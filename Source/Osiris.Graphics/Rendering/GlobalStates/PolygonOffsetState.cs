using System;

namespace Osiris.Graphics.Rendering.GlobalStates
{
	public class PolygonOffsetState : GlobalState
	{
		// Set whether offset should be enabled for the various polygon drawing
		// modes (fill).
		public bool FillEnabled;

		// The offset is Scale*dZ + Bias*r where dZ is the change in depth
		// relative to the screen space area of the poly, and r is the smallest
		// resolvable depth difference.  Negative values move polygons closer to
		// the eye.
		public float Scale;  // default: 0.0
		public float Bias;   // default: 0.0

		public override StateType Type
		{
			get { return StateType.PolygonOffset; }
		}

		static PolygonOffsetState()
		{
			GlobalStateCollection.Default[StateType.PolygonOffset] = new PolygonOffsetState();
		}

		public PolygonOffsetState()
		{
			FillEnabled = false;
			Scale = 0.0f;
			Bias = 0.0f;
		}
	}
}
