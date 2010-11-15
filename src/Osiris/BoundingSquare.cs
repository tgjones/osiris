using System;
using Microsoft.Xna.Framework;

namespace Osiris
{
	public struct BoundingSquare
	{
		#region Fields

		public IntVector2 Min;
		public IntVector2 Max;

		#endregion

		#region Properties

		public IntVector2 BottomEdgeMidpoint
		{
			get { return new IntVector2(Min.X + ((Max.X - Min.X) / 2), Min.Y); }
		}

		public IntVector2 TopEdgeMidpoint
		{
			get { return new IntVector2(Min.X + ((Max.X - Min.X) / 2), Max.Y); }
		}

		public IntVector2 LeftEdgeMidpoint
		{
			get { return new IntVector2(Min.X, Min.Y + ((Max.Y - Min.Y) / 2)); }
		}

		public IntVector2 RightEdgeMidpoint
		{
			get { return new IntVector2(Max.X, Min.Y + ((Max.Y - Min.Y) / 2)); }
		}

		public int EdgeLength
		{
			get { return Max.X - Min.X; }
		}

		#endregion

		public BoundingSquare(IntVector2 min, IntVector2 max)
		{
			Min = min;
			Max = max;
		}
	}
}
