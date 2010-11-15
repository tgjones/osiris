using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris
{
	[StructLayout(LayoutKind.Sequential)]
	public struct ShortVector2
	{
		public short X;
		public short Y;

		public unsafe static int SizeInBytes
		{
			get { return sizeof(ShortVector2); }
		}

		public static readonly VertexElement[] VertexElements = new VertexElement[]
		{
			new VertexElement(0, 0, VertexElementFormat.Short2, VertexElementMethod.Default, VertexElementUsage.Position, 0),
		};

		/// <summary>
		/// Creates a new instance of ShortVector2.
		/// </summary>
		/// <param name="x">Initial value for the x component of the vector.</param>
		/// <param name="y">Initial value for the y component of the vector.</param>
		public ShortVector2(short x, short y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// Creates a new instance of ShortVector2.
		/// </summary>
		/// <param name="value">Value to initialise both components to.</param>
		public ShortVector2(short value)
		{
			X = value;
			Y = value;
		}

		public override string ToString()
		{
			return "X: " + X + ", Y: " + Y;
		}

		public static explicit operator ShortVector2(IntVector2 v)
		{
			return new ShortVector2((short) v.X, (short) v.Y);
		}
	}
}
