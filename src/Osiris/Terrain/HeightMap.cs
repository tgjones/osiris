using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Osiris.Sky;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Terrain
{
	public abstract class HeightMap : GameComponent, IHeightMapService
	{
		private bool _applyBoxFilter;
		//private bool _seamless;

		protected float[,] m_pHeights;
		protected int m_nWidth;
		protected int m_nHeight;

		protected float m_fMaximumHeight;

		#region Properties

		/// <summary>
		/// Retrieves the height for the specified coordinates
		/// </summary>
		public float this[int nX, int nY]
		{
			get { return m_pHeights[nX, nY]; }
		}

		/// <summary>
		/// Calculates heights in-between actual vertices by interpolating
		/// heights at the surround vertices
		/// </summary>
		public float this[float fMapX, float fMapY]
		{
			get
			{
				// convert coordinates to heightmap scale
				//fMapX *= m_nWidth - 1;
				//fMapY *= m_nHeight - 1;

				// get integer and fractional parts of coordinates
				int nIntX0 = MathsHelper.FloorToInt(fMapX);
				int nIntY0 = MathsHelper.FloorToInt(fMapY);
				float fFractionalX = fMapX - nIntX0;
				float fFractionalY = fMapY - nIntY0;

				// get coordinates for "other" side of quad
				int nIntX1 = MathsHelper.Clamp(nIntX0 + 1, 0, m_nWidth - 1);
				int nIntY1 = MathsHelper.Clamp(nIntY0 + 1, 0, m_nHeight - 1);

				// read 4 map values
				float f0 = this[nIntX0, nIntY0];
				float f1 = this[nIntX1, nIntY0];
				float f2 = this[nIntX0, nIntY1];
				float f3 = this[nIntX1, nIntY1];

				// calculate averages
				float fAverageLo = (f1 * fFractionalX) + (f0 * (1.0f - fFractionalX));
				float fAverageHi = (f3 * fFractionalX) + (f2 * (1.0f - fFractionalX));

				return (fAverageHi * fFractionalY) + (fAverageLo * (1.0f - fFractionalY));
			}
		}

		public float MaximumHeight
		{
			get { return m_fMaximumHeight; }
		}

		public int Width
		{
			get { return m_nWidth; }
		}

		public int Height
		{
			get { return m_nHeight; }
		}

		#endregion

		public HeightMap(Game game, bool applyBoxFilter)
			: base(game)
		{
			_applyBoxFilter = applyBoxFilter;
			//_seamless = seamless;
		}

		#region Methods

		public Texture2D GetTexture()
		{
			Texture2D texture = new Texture2D(Game.GraphicsDevice, m_nWidth, m_nHeight, 1, TextureUsage.None, SurfaceFormat.Single);
			float[] heights = new float[m_pHeights.LongLength]; int counter = 0;
			for (int y = 0; y < m_nHeight; ++y)
				for (int x = 0; x < m_nWidth; ++x)
					heights[counter++] = m_pHeights[x, y];
			texture.SetData<float>(heights);
			return texture;
		}

		public Vector3 GetPosition(int nX, int nY)
		{
			return new Vector3(nX, this[nX, nY], -nY);
		}

		public override void Initialize()
		{
			if (_applyBoxFilter)
			{
				ApplyBoxFilter();
				ApplyBoxFilter();
			}

			/*if (_seamless)
				MakeSeamless();*/

			base.Initialize();
		}

		/// <summary>
		/// http://www.moon-labs.com/resources/TerrainPart2.pdf
		/// 
		/// Function computes the average height of the ij element.
		/// It averages itself with its eight neighbor pixels. Note
		/// that if a pixel is missing neighbor, we just don't include it
		/// in the average--that is, edge pixels don't have a neighbor
		/// pixel.
		/// 
		/// ----------
		/// | 7| 8| 9|
		/// ----------
		/// |4 |xy| 6|
		/// ----------
		/// | 1| 2| 3|
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private float SampleHeight3x3(int x, int y)
		{
			float avg = 0, num = 0;
			for (int m = x - 1; m <= x + 1; ++m)
				for (int n = y - 1; n <= y + 1; ++n)
					if (InBounds(m, n))
					{
						avg += this[m, n];
						num += 1;
					}
			return avg / num;
		}

		private bool InBounds(int x, int y)
		{
			return (x >= 0 && x < m_nWidth && y >= 0 && y < m_nHeight);
		}

		private void ApplyBoxFilter()
		{
			float[,] processedHeights = new float[m_nWidth, m_nHeight];
			for (int y = 0; y < m_nHeight; y++)
				for (int x = 0; x < m_nWidth; x++)
					processedHeights[x, y] = SampleHeight3x3(x, y);
			m_pHeights = processedHeights;
		}

		/*private void MakeSeamless()
		{
			const int THRESHOLD = m_nWidth / 10;

			float[,] processedHeights = new float[m_nWidth, m_nHeight];
			for (int y = 0; y < m_nHeight; y++)
				for (int x = 0; x < m_nWidth; x++)
					if (y > m_nHeight - THRESHOLD || x > m_nWidth - THRESHOLD)
					{
						IntVector2 alpha = new IntVector2(
							(THRESHOLD - (m_nWidth - x)) / (float) THRESHOLD,
							(THRESHOLD - (m_nHeight - y)) / (float) THRESHOLD);
						float maxAlpha = Math.Max(alpha.X, alpha.Y);
						processedHeights[x, y] = m_pHeights[x, y] * (1.0f - maxAlpha) + (m_pHeights[x, * maxAlpha);
					}
			m_pHeights = processedHeights;
		}*/

		#endregion
	}
}
