using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Osiris.Maths
{
	/// <summary>
	/// Implementing Improved Perlin Noise, Simon Green, GPU Gems 2
	/// with help from Simplex Noise Demystified
	/// </summary>
	public class Noise : GameComponent, INoiseService
	{
		private static byte[] _permutations = new byte[]
		{
			151,160,137,91,90,15,
			131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
			190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
			88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
			77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
			102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
			135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
			5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
			223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
			129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
			251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
			49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
			138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
		};

		private static Vector2[] _gradients2D = new Vector2[]
		{
			new Vector2(1,1),
			new Vector2(-1,1),
			new Vector2(1,-1),
			new Vector2(-1,-1),
			new Vector2(1,0),
			new Vector2(-1,0),
			new Vector2(1,0),
			new Vector2(-1,0), 
			new Vector2(0,1),
			new Vector2(0,-1),
			new Vector2(0,1),
			new Vector2(0,-1),
			new Vector2(1,1),
			new Vector2(0,-1),
			new Vector2(-1,1),
			new Vector2(0,-1)
		};

		/// <summary>
		/// Generates 16 gradients distributed evenly around the unit circle
		/// </summary>
		static Noise()
		{
			_gradients2D = new Vector2[16];
			float delta = MathHelper.TwoPi / 16.0f;
			float current = 0;
			for (int i = 0; i < 16; i++)
			{
				_gradients2D[i].X = MathsHelper.Sin(current);
				_gradients2D[i].Y = MathsHelper.Cos(current);
				current += delta;
			}
		}

		public Noise(Game game)
			: base(game)
		{
			game.Services.AddService(typeof(INoiseService), this);
			game.Components.ComponentRemoved += new EventHandler<GameComponentCollectionEventArgs>(Components_ComponentRemoved);
		}

		private void Components_ComponentRemoved(object sender, GameComponentCollectionEventArgs e)
		{
			if (e.GameComponent == this)
				Game.Services.RemoveService(typeof(INoiseService));
		}

		public Texture2D GetPermutationTexture()
		{
			Texture2D texture = new Texture2D(Game.GraphicsDevice, 256, 1, 1, TextureUsage.None, SurfaceFormat.Luminance8);
			texture.SetData<byte>(_permutations);
			return texture;
		}

		public Texture2D GetGradientTexture()
		{
			// copy 2D array into 1D array
			NormalizedByte2[] gradients2D = new NormalizedByte2[_gradients2D.Length];
			for (int i = 0, length = _gradients2D.Length; i < length; i++)
				gradients2D[i] = new NormalizedByte2(_gradients2D[i]);

			Texture2D texture = new Texture2D(Game.GraphicsDevice, _gradients2D.Length, 1, 1, TextureUsage.None, SurfaceFormat.NormalizedByte2);
			texture.SetData<NormalizedByte2>(gradients2D);
			return texture;
		}

		#region CPU implementation of Perlin noise, which exactly matches the GPU implementation

		private static float Lerp(float value1, float value2, float amount)
		{
			return MathHelper.Lerp(value1, value2, amount);
		}

		private static Vector2 Fade(Vector2 t)
		{
			return t * t * t * (t * (t * 6 - new Vector2(15)) + new Vector2(10)); // new curve: t^3(t(6t-15)+10) = t^3(6t^2-15t+10) = 6t^5 - 15t^4 + 10t^3
			// return t * t * (3 - 2 * t); // old curve: t^2(3-2t) = 3t^2 - 2t^3
		}

		private static byte Perm(int x)
		{
			// CHANGE
			x = x % _permutations.Length;
			if (x < 0) x = _permutations.Length + x;
			return _permutations[x];
		}

		private static float Grad(int x, Vector2 p)
		{
			// CHANGE
			x = x % _gradients2D.Length;
			return Vector2.Dot(_gradients2D[x], p);
		}

		private static float PerlinNoise(Vector2 p)
		{
			// get integer and fractional parts of p
			IntVector2 pi = (IntVector2) p;
			Vector2 pf = p - pi;

			// calculate interpolation factor (fade curve)
			Vector2 f = Fade(pf);

			// hash coordinates for two of the four square corners
			int A = Perm(pi.X) + pi.Y;
			int B = Perm(pi.X + 1) + pi.Y;

			// add blended results from 4 corners of square
			float result = Lerp(Lerp(Grad(Perm(A), pf),
															 Grad(Perm(B), pf + new Vector2(-1, 0)),
															 f.X),
													Lerp(Grad(Perm(A + 1), pf + new Vector2(0, -1)),
															 Grad(Perm(B + 1), pf + new Vector2(-1, -1)),
															 f.X),
													f.Y);

			return result;
		}

		// fractal sum

		public static float GenerateFBM(Vector2 p, int octaves)
		{
			return GenerateFBM(p, octaves, 2.0f, 0.5f);
		}

		public static float GenerateFBM(Vector2 p, int octaves, float lacunarity, float gain)
		{
			float freq = 1.0f, amp = 0.5f;
			float sum = 0;
			for (int i = 0; i < octaves; i++)
			{
				sum += PerlinNoise(p * freq) * amp;
				freq *= lacunarity;
				amp *= gain;
			}
			return sum;
		}

		// perlin turbulence

		public static float GenerateTurbulence(Vector2 p, int octaves)
		{
			return GenerateTurbulence(p, octaves, 2.0f, 0.5f);
		}

		public static float GenerateTurbulence(Vector2 p, int octaves, float lacunarity, float gain)
		{
			float sum = 0;
			float freq = 1.0f, amp = 1.0f;
			for (int i = 0; i < octaves; i++)
			{
				sum += Math.Abs(PerlinNoise(p * freq)) * amp;
				freq *= lacunarity;
				amp *= gain;
			}
			return sum;
		}

		// ridged multifractal

		private static float Ridge(float h, float offset)
		{
			h = Math.Abs(h);
			h = offset - h;
			h = h * h;
			return h;
		}

		public static float GenerateRidgedMultifractal(Vector2 p, int octaves)
		{
			return GenerateRidgedMultifractal(p, octaves, 2.0f, 0.5f, 1.0f);
		}

		public static float GenerateRidgedMultifractal(Vector2 p, int octaves, float lacunarity, float gain, float offset)
		{
			float sum = 0;
			float freq = 1.0f, amp = 0.5f;
			float prev = 1.0f;
			for (int i = 0; i < octaves; i++)
			{
				float n = Ridge(PerlinNoise(p * freq), offset);
				sum += n * amp * prev;
				prev = n;
				freq *= lacunarity;
				amp *= gain;
			}
			return sum;
		}

		public static float GenerateHeteroTerrain(Vector2 p, int octaves)
		{
			return GenerateHeteroTerrain(p, octaves, 2.0f, 0.5f);
		}

		public static float GenerateHeteroTerrain(Vector2 p, int octaves, float lacunarity, float gain)
		{
			float sum = 1;
			float freq = 1.0f, amp = 0.5f;
			for (int i = 0; i < octaves; i++)
			{
				float n = PerlinNoise(p * freq) * amp * sum;
				sum += n;
				freq *= lacunarity;
				amp *= gain;
			}
			return sum;
		}

		#endregion
	}
}
