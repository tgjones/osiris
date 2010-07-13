using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Short2
	{
		public short X;
		public short Y;

		public unsafe static int SizeInBytes
		{
			get { return sizeof(Short2); }
		}

		/// <summary>
		/// Creates a new instance of ShortVector2.
		/// </summary>
		/// <param name="x">Initial value for the x component of the vector.</param>
		/// <param name="y">Initial value for the y component of the vector.</param>
		public Short2(short x, short y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// Creates a new instance of ShortVector2.
		/// </summary>
		/// <param name="value">Value to initialise both components to.</param>
		public Short2(short value)
		{
			X = value;
			Y = value;
		}

		public override string ToString()
		{
			return "X: " + X + ", Y: " + Y;
		}

		public static explicit operator Short2(Int2 v)
		{
			return new Short2((short) v.X, (short) v.Y);
		}
	}
}
