using System;
using Microsoft.Xna.Framework;

namespace Osiris
{
	public static class MathUtility
	{
		public static int FloorToInt(float f)
		{
			return (int) Math.Floor(f);
		}

		public static float Floor(float f)
		{
			return (float) Math.Floor(f);
		}

		public static int Pow(int x, int y)
		{
			return (int) Math.Pow(x, y);
		}

		public static float Pow(float x, float y)
		{
			return (float) Math.Pow(x, y);
		}

		public static Vector3 Pow(Vector3 v, float y)
		{
			return new Vector3(
				Pow(v.X, y),
				Pow(v.Y, y),
				Pow(v.Z, y));
		}

		public static float Sqrt(float f)
		{
			return (float) Math.Sqrt(f);
		}

		public static int CeilingToInt(float f)
		{
			return (int) Math.Ceiling(f);
		}

		public static float Cos(float a)
		{
			return (float) Math.Cos(a);
		}

		public static float Sin(float a)
		{
			return (float) Math.Sin(a);
		}

		public static float Tan(float a)
		{
			return (float) Math.Tan(a);
		}

		public static float Atan2(float y, float x)
		{
			return (float) Math.Atan2(y, x);
		}

		public static int Clamp(int value, int min, int max)
		{
			value = (value > max) ? max : value;
			value = (value < min) ? min : value;
			return value;
		}

		/// <summary>
		/// Calculates the weight of a given value between the min and max specified
		/// </summary>
		/// <param name="fValue"></param>
		/// <param name="fMin"></param>
		/// <param name="fMax"></param>
		/// <returns></returns>
		public static float ComputeWeight(float fValue, float fMin, float fMax)
		{
			float fWeight = 0.0f;

			if (fValue >= fMin && fValue <= fMax)
			{
				float fSpan = fMax - fMin;
				fWeight = fValue - fMin;

				// convert to a -1 to 1 range between min and max
				fWeight /= fSpan;
				fWeight -= 0.5f;
				fWeight *= 2.0f;

				// square result for non-linear falloff
				fWeight *= fWeight;

				// invert result
				fWeight = 1.0f - fWeight;
			}

			return fWeight;
		}

		public static Vector3 Invert(Vector3 v)
		{
			return new Vector3(
				1.0f / v.X,
				1.0f / v.Y,
				1.0f / v.Z);
		}

		public static float Exp(float f)
		{
			return (float) Math.Exp(f);
		}

		/// <summary>
		/// http://local.wasp.uwa.edu.au/~pbourke/other/interpolation/
		/// Cosine interpolation
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="x"></param>
		/// <returns></returns>
		public static float InterpolateCosine(float y1, float y2, float mu)
		{
			float f = (1 - Cos(mu * Microsoft.Xna.Framework.MathHelper.Pi)) * 0.5f;
			return (y1 * (1 - f)) + (y2 * f);
		}

		public static float InterpolateLinear(float y1, float y2, float mu)
		{
			return (y1 * (1 - mu)) + (y2 * mu);
		}

		/// <summary>
		/// v(u) = v(0) - (3u^2 - 2u^3)*(v(0) - v(1))
		/// 
		/// y0 and y3 are points either side of y1 and y2, which represent the range we're interpolating
		/// </summary>
		/// <param name="y0"></param>
		/// <param name="y1"></param>
		/// <param name="y2"></param>
		/// <param name="y3"></param>
		/// <param name="mu"></param>
		/// <returns></returns>
		public static float InterpolateCubic(
			float y0, float y1,
			float y2, float y3,
			float mu)
		{
			float mu2 = mu * mu;
			float a0 = y3 - y2 - y0 + y1;
			float a1 = y0 - y1 - a0;
			float a2 = y2 - y0;
			float a3 = y1;

			return (a0 * mu * mu2 + a1 * mu2 + a2 * mu + a3);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="y0"></param>
		/// <param name="y1"></param>
		/// <param name="y2"></param>
		/// <param name="y3"></param>
		/// <param name="mu"></param>
		/// <param name="tension">1 is high, 0 normal, -1 is low</param>
		/// <param name="bias">0 is even, positive towards first segment, negative towards the other</param>
		/// <returns></returns>
		public static float InterpolateHermite(
			float y0, float y1,
			float y2, float y3,
			float mu,
			float tension,
			float bias)
		{
			float mu2 = mu * mu;
			float mu3 = mu2 * mu;
			float m0 = (y1 - y0) * (1 + bias) * (1 - tension) / 2;
			m0 += (y2 - y1) * (1 - bias) * (1 - tension) / 2;
			float m1 = (y2 - y1) * (1 + bias) * (1 - tension) / 2;
			m1 += (y3 - y2) * (1 - bias) * (1 - tension) / 2;
			float a0 = 2 * mu3 - 3 * mu2 + 1;
			float a1 = mu3 - 2 * mu2 + mu;
			float a2 = mu3 - mu2;
			float a3 = -2 * mu3 + 3 * mu2;

			return (a0 * y1 + a1 * m0 + a2 * m1 + a3 * y2);
		}
	}
}