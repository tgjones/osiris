using System;
using Microsoft.Xna.Framework;

namespace Osiris
{
	public struct Int2
	{
		public int X;
		public int Y;

		public static Int2 Zero
		{
			get { return new Int2(0, 0); }
		}

		/// <summary>
		/// Creates a new instance of Int2.
		/// </summary>
		/// <param name="x">Initial value for the x component of the vector.</param>
		/// <param name="y">Initial value for the y component of the vector.</param>
		public Int2(int x, int y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// Creates a new instance of Int2.
		/// </summary>
		/// <param name="value">Value to initialise both components to.</param>
		public Int2(int value)
		{
			X = value;
			Y = value;
		}

		public override string ToString()
		{
			return "X: " + X + ", Y: " + Y;
		}

		#region Operators

		public static Int2 operator +(Int2 a, Int2 b)
		{
			return new Int2(a.X + b.X, a.Y + b.Y);
		}

		public static Int2 operator -(Int2 a, Int2 b)
		{
			return new Int2(a.X - b.X, a.Y - b.Y);
		}

		public static Int2 operator *(Int2 a, int n)
		{
			return new Int2(a.X * n, a.Y * n);
		}

		public static Int2 operator /(Int2 a, int n)
		{
			return new Int2(a.X / n, a.Y / n);
		}

		public static bool operator ==(Int2 a, Int2 b)
		{
			return (a.X == b.X) && (a.Y == b.Y);
		}

		public static bool operator !=(Int2 a, Int2 b)
		{
			return !(a == b);
		}

		public static bool operator <=(Int2 a, Int2 b)
		{
			return (a.X <= b.X && a.Y <= b.Y);
		}

		public static bool operator >=(Int2 a, Int2 b)
		{
			return (a.X >= b.X && a.Y >= b.Y);
		}

		public static explicit operator Int2(Vector2 v)
		{
			return new Int2((int) v.X, (int) v.Y);
		}

		public static implicit operator Vector2(Int2 v)
		{
			return new Vector2(v.X, v.Y);
		}

		public static Int2 operator %(Int2 a, int n)
		{
			return new Int2(a.X % n, a.Y % n);
		}

		#endregion

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			if (!(obj is Int2))
				return false;

			Int2 lValue = (Int2) obj;
			return (X == lValue.X && Y == lValue.Y);
		}

		public override int GetHashCode()
		{
			return X ^ Y;
		}
	}
}