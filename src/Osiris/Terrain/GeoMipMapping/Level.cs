using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics.Cameras;

namespace Osiris.Terrain.GeoMipMapping
{
	/// <summary>
	/// Summary description for Level.
	/// </summary>
	public class Level
	{
		#region Fields

		private const int TOTAL_NEIGHBOUR_CODES = 16;

		private GeoMipMappedTerrain _terrain;
		private bool _oneTimeInitialised = false;

		private static int[][] m_pNumPrimitives;
		private static int[][] m_pNumIndices;
		private static short[][][] m_pIndices;

		private static IndexBuffer[][] m_pSharedIndexBuffers;

		private static float m_fC = float.MinValue;

		private int m_nNeighboursCode;
		private int m_nLevel;
		
		private float m_fMaximumDelta = 0.0f;
		private float m_fMinimumDSq = 0.0f;

		#endregion

		#region Properties

		public int NeighboursCode
		{
			get {return m_nNeighboursCode;}
			set {m_nNeighboursCode = value;}
		}

		public float MinimumDSq
		{
			get {return m_fMinimumDSq;}
		}

		#endregion

		#region Constructors

		#region Instance constructor

		public Level(Game game, GeoMipMappedTerrain terrain, GraphicsDevice graphicsDevice, int nLevel, IHeightMapService pHeightMap,
			int nStartX, int nEndX, int nStartY, int nEndY)
		{
			_terrain = terrain;

			if (!_oneTimeInitialised)
			{
				m_pNumPrimitives = new int[Patch.NumLevels][];
				m_pNumIndices = new int[Patch.NumLevels][];
				m_pIndices = new short[Patch.NumLevels][][];

				for (short i = 0; i < Patch.NumLevels; i++)
				{
					// calculate number of vertices
					int nNumVerticesOneSide = GetPowerOfTwo((short) (Patch.NumLevels - 1 - i)) + 1;
					int nNumVertices = nNumVerticesOneSide * nNumVerticesOneSide;

					// calculate number of primitives
					int nNumRows = nNumVerticesOneSide - 1;

					// for level 0, we only have set of indices because we never adapt to neighbours
					int nTotalNeighbourCodes = (i == 0) ? 1 : TOTAL_NEIGHBOUR_CODES;
					m_pNumPrimitives[i] = new int[nTotalNeighbourCodes];
					m_pNumIndices[i] = new int[nTotalNeighbourCodes];
					m_pIndices[i] = new short[nTotalNeighbourCodes][];

					short hSkip = GetPowerOfTwo(i);
					short hHalfSkip = (short) (hSkip / 2);

					for (int j = 0; j < nTotalNeighbourCodes; j++)
					{
						bool bLeft, bRight, bTop, bBottom;
						Patch.GetNeighboursBoolean(j, out bLeft, out bRight, out bTop, out bBottom);

						// generate indices
						if (i == Patch.NumLevels - 1)
						{
							m_pIndices[i][j] = AddHighestLevelTriangles(hSkip, hHalfSkip, j);
						}
						else
						{
							IndexBuilder pIndexBuilder = new IndexBuilder(_terrain);
							for (int y = 0; y < _terrain.PatchSize - hSkip; y += hSkip)
								AddTriangleRow(pIndexBuilder, y, hSkip, hHalfSkip, bLeft, bRight, bTop, bBottom);
							m_pIndices[i][j] = pIndexBuilder.Indices;
						}

						// set number of indices
						m_pNumIndices[i][j] = m_pIndices[i][j].Length;

						// calculate number of primitives
						m_pNumPrimitives[i][j] = m_pNumIndices[i][j] - 2;
					}
				}

				_oneTimeInitialised = true;
			}

			m_nLevel = nLevel;

			#region Shared index buffers

			if (m_pSharedIndexBuffers == null)
			{
				m_pSharedIndexBuffers = new IndexBuffer[Patch.NumLevels][];

				for (int i = 0; i < Patch.NumLevels; i++)
				{
					int nTotalNeighbourCodes = (i == 0) ? 1 : TOTAL_NEIGHBOUR_CODES;
					m_pSharedIndexBuffers[i] = new IndexBuffer[nTotalNeighbourCodes];
					for (int j = 0; j < nTotalNeighbourCodes; j++)
					{
						m_pSharedIndexBuffers[i][j] = new IndexBuffer(graphicsDevice, typeof(short),
							m_pNumIndices[i][j], BufferUsage.None);
						m_pSharedIndexBuffers[i][j].SetData<short>(m_pIndices[i][j]);
					}
				}
			}

			#endregion

			#region Geomipmapping calculations

			if (nLevel > 0) // don't need to calculate for top level, since it's "minimum d" is always 0
			{
				short hSkip = GetPowerOfTwo((short) nLevel);
				short hHalfSkip = (short) (hSkip / 2);
				for (int y = nStartY; y < nEndY - 1; y += hSkip)
				{
					for (int x = nStartX; x < nEndX; x += hSkip)
					{
						// down-left
						if (x > nStartX)
							CalculateDelta(pHeightMap[x - hHalfSkip, y + hHalfSkip], pHeightMap[x, y], pHeightMap[x - hSkip, y + hSkip]);

						// down
						CalculateDelta(pHeightMap[x, y + hHalfSkip], pHeightMap[x, y], pHeightMap[x, y + hSkip]);

						// down-right
						if (x < nEndX - 1)
							CalculateDelta(pHeightMap[x + hHalfSkip, y + hHalfSkip], pHeightMap[x, y], pHeightMap[x + hSkip, y + hSkip]);

						// right
						if (x < nEndX - 1)
							CalculateDelta(pHeightMap[x + hHalfSkip, y], pHeightMap[x, y], pHeightMap[x + hSkip, y]);
					}
				}
			}

			RecalculateMinimumD(_terrain.Tau, (ICameraService) game.Services.GetService(typeof(ICameraService)), graphicsDevice);

			#endregion
		}

		#endregion

		#endregion

		#region Methods

		public void RecalculateMinimumD(float tau, ICameraService camera, GraphicsDevice graphicsDevice)
		{
			// precalculate C
			float fA = camera.ProjectionNear / Math.Abs(camera.ProjectionTop); // 2
			float fT = 2 * tau / (float) graphicsDevice.Viewport.Height; // 0.01333
			m_fC = fA / fT; // 150

			// we now have maximum delta
			float fMinimumD = m_fMaximumDelta * m_fC;
			m_fMinimumDSq = fMinimumD * fMinimumD;
		}

		public void Draw(GraphicsDevice graphicsDevice)
		{
			graphicsDevice.Indices = m_pSharedIndexBuffers[m_nLevel][m_nNeighboursCode];

			graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip,
				0,                           // base vertex
				0,                           // min vertex index
				Patch.NumVertices,           // total num vertices - note that is NOT just vertices that are indexed, but all vertices
				0,                           // start index
				m_pNumPrimitives[m_nLevel][m_nNeighboursCode]); // primitive count
		}

		#region Loadtime methods

		#region AddTriangleRow method

		private void AddTriangleRow(IndexBuilder pIndexBuilder,
			int nY, short hSkip, short hHalfSkip,
			bool bLeft, bool bRight, bool bTop, bool bBottom)
		{
			// special case for top row when patch above is more detailed
			if (bTop && nY == 0)
			{
				AddTrianglesTopRow(pIndexBuilder, hSkip, hHalfSkip, bLeft, bRight);
				return;
			}

			// if this is not the first row, add index for degenerate triangle
			// coming from previous row (this is so that we can keep the triangle list going)
			if (nY != 0)
			{
				pIndexBuilder.AddIndex(0, nY + hSkip);
			}

			// special case for bottom row when patch below is more detailed
			if (bBottom && (nY == _terrain.PatchSize - 1 - hSkip))
			{
				AddTrianglesBottomRow(pIndexBuilder, nY, hSkip, hHalfSkip, bLeft, bRight);
				return;
			}

			int nStartX = 0, nEndX = _terrain.PatchSize;

			// special case if left patch (or left and top patch, or left and bottom patch) is more detailed
			if (bLeft)
			{
				pIndexBuilder.AddIndex(0, nY + hSkip);
				pIndexBuilder.AddIndex(0, nY + hHalfSkip);
				pIndexBuilder.AddIndex(hSkip, nY + hSkip);
				pIndexBuilder.AddIndex(0, nY);
				pIndexBuilder.AddIndex(hSkip, nY);
				pIndexBuilder.AddLastIndexAgain();

				// shift starting x pos
				nStartX = hSkip;
			}

			// if right patch is more detailed, don't fill the end block normally
			if (bRight)
			{
				nEndX -= hSkip;
			}

			// standard row filler
			for (int x = nStartX; x < nEndX; x += hSkip)
			{
				pIndexBuilder.AddIndex(x, nY + hSkip);
				pIndexBuilder.AddIndex(x, nY);
			}

			// special case if right patch is more detailed
			if (bRight)
			{
				int nRightX = _terrain.PatchSize - 1 - hSkip;
				pIndexBuilder.AddIndex(nRightX, nY + hSkip);
				pIndexBuilder.AddLastIndexAgain();
				pIndexBuilder.AddIndex(nRightX + hSkip, nY + hSkip);
				pIndexBuilder.AddIndex(nRightX, nY);
				pIndexBuilder.AddIndex(nRightX + hSkip, nY + hHalfSkip);
				pIndexBuilder.AddIndex(nRightX + hSkip, nY);
			}

			// if this is not last row, add extra index for degenerate triangle
			if (nY != _terrain.PatchSize - 1 - hSkip)
			{
				pIndexBuilder.AddLastIndexAgain();
			}
		}

		/// <summary>
		/// Used when the patch above is more detailed, and we are filling the top row
		/// </summary>
		/// <param name="pIndexBuilder"></param>
		/// <param name="hSkip"></param>
		/// <param name="hHalfSkip"></param>
		/// <param name="bLeft"></param>
		/// <param name="bRight"></param>
		private void AddTrianglesTopRow(IndexBuilder pIndexBuilder, short hSkip, short hHalfSkip,
			bool bLeft, bool bRight)
		{
			int nStartX = 0, nEndX = _terrain.PatchSize - 1;

			if (bLeft)
			{
				pIndexBuilder.AddIndex(0, 0);
				pIndexBuilder.AddIndex(hHalfSkip, 0);
				pIndexBuilder.AddIndex(0, hHalfSkip);
				pIndexBuilder.AddIndex(hSkip, 0);
				pIndexBuilder.AddIndex(0, hSkip);
				pIndexBuilder.AddIndex(hSkip, hSkip);

				nStartX = hSkip;
			}

			// if right patch is more detailed, don't fill the end block normally
			if (bRight)
			{
				nEndX -= hSkip;
			}

			for (int x = nStartX; x < nEndX; x += hSkip)
			{
				pIndexBuilder.AddIndex(x, 0);
				pIndexBuilder.AddIndex(x + hHalfSkip, 0);
				pIndexBuilder.AddIndex(x, hSkip);
				pIndexBuilder.AddIndex(x + hSkip, 0);
				pIndexBuilder.AddIndex(x + hSkip, hSkip);
				if (x != _terrain.PatchSize - 1)
				{
					pIndexBuilder.AddLastIndexAgain();
				}
				if (x == _terrain.PatchSize - hSkip - 1)
				{
					pIndexBuilder.AddLastIndexAgain();
				}
			}

			if (bRight)
			{
				int nRightX = _terrain.PatchSize - 1 - hSkip;
				pIndexBuilder.AddIndex(nRightX, 0);
				pIndexBuilder.AddLastIndexAgain();
				pIndexBuilder.AddIndex(nRightX, hSkip);
				pIndexBuilder.AddIndex(nRightX + hHalfSkip, 0);
				pIndexBuilder.AddIndex(nRightX + hSkip, 0);
				pIndexBuilder.AddLastIndexAgain();
				pIndexBuilder.AddIndex(nRightX, hSkip);
				pIndexBuilder.AddIndex(nRightX + hSkip, hHalfSkip);
				pIndexBuilder.AddIndex(nRightX + hSkip, hSkip);
			}

			// don't need to add degenerate triangle due to weird indexing
		}

		/// <summary>
		/// Used when the patch below is more detailed, and we are filling the bottom row
		/// </summary>
		/// <param name="pIndexBuilder"></param>
		/// <param name="hSkip"></param>
		/// <param name="hHalfSkip"></param>
		/// <param name="bLeft"></param>
		/// <param name="bRight"></param>
		private void AddTrianglesBottomRow(IndexBuilder pIndexBuilder,
			int nY, short hSkip, short hHalfSkip,
			bool bLeft, bool bRight)
		{
			int nStartX = 0, nEndX = _terrain.PatchSize - 1;

			if (bLeft)
			{
				pIndexBuilder.AddIndex(0, nY + hSkip);
				pIndexBuilder.AddIndex(0, nY + hHalfSkip);
				pIndexBuilder.AddIndex(hHalfSkip, nY + hSkip);
				pIndexBuilder.AddIndex(0, nY);
				pIndexBuilder.AddIndex(hSkip, nY + hSkip);
				pIndexBuilder.AddIndex(hSkip, nY);

				nStartX = hSkip;
			}

			// if right patch is more detailed, don't fill the end block normally
			if (bRight)
			{
				nEndX -= hSkip;
			}

			for (int x = nStartX; x < nEndX; x += hSkip)
			{
				pIndexBuilder.AddIndex(x, nY + hSkip);
				pIndexBuilder.AddIndex(x, nY);
				pIndexBuilder.AddIndex(x + hHalfSkip, nY + hSkip);
				pIndexBuilder.AddIndex(x + hSkip, nY);
				pIndexBuilder.AddIndex(x + hSkip, nY + hSkip);
				if (x != _terrain.PatchSize - 1 - hSkip)
				{
					pIndexBuilder.AddLastIndexAgain();
				}
			}

			if (bRight)
			{
				int nRightX = _terrain.PatchSize - 1 - hSkip;
				pIndexBuilder.AddIndex(nRightX, nY + hSkip);
				pIndexBuilder.AddIndex(nRightX, nY);
				pIndexBuilder.AddIndex(nRightX + hHalfSkip, nY + hSkip);
				pIndexBuilder.AddIndex(nRightX + hSkip, nY + hSkip);
				pIndexBuilder.AddLastIndexAgain();
				pIndexBuilder.AddIndex(nRightX, nY);
				pIndexBuilder.AddIndex(nRightX + hSkip, nY + hHalfSkip);
				pIndexBuilder.AddIndex(nRightX + hSkip, nY);
			}
		}

		#endregion

		#region AddHighestLevelTriangles method

		/// <summary>
		/// For a 17*17 patch, this creates the indices for Level 4
		/// </summary>
		/// <param name="hSkip"></param>
		/// <param name="hHalfSkip"></param>
		/// <param name="nCode">Bitmask representing neighbour codes</param>
		/// <returns></returns>
		private short[] AddHighestLevelTriangles(short hSkip, short hHalfSkip, int nCode)
		{
			// declare helper variables, for clarification (not the fastest, but I'd rather it was clear)
			short[] pTopLeft     = new short[] {0, 0};
			short[] pTopRight    = new short[] {hSkip, 0};
			short[] pBottomLeft  = new short[] {0, hSkip};
			short[] pBottomRight = new short[] {hSkip, hSkip};
			short[] pMidLeft     = new short[] {0, hHalfSkip};
			short[] pMidRight    = new short[] {hSkip, hHalfSkip};
			short[] pMidTop      = new short[] {hHalfSkip, 0};
			short[] pMidBottom   = new short[] {hHalfSkip, hSkip};

			switch (nCode)
			{
				case 0 : // all patches same LOD
					return CreateIndices(
						pTopLeft,
						pTopRight,
						pBottomLeft,
						pBottomRight);
				case 1 : // left patch higher LOD
					return CreateIndices(
						pTopLeft,
						pTopRight,
						pMidLeft,
						pBottomRight,
						pBottomLeft);
				case 2 : // right patch higher LOD
					return CreateIndices(
						pTopLeft,
						pTopRight,
						pBottomLeft,
						pMidRight,
						pBottomRight);
				case 3 : // left and right patches higher LOD
					return CreateIndices(
						pTopLeft,
						pTopRight,
						pMidLeft,
						pMidRight,
						pBottomLeft,
						pBottomRight);
				case 4 : // top patch higher LOD
					return CreateIndices(
						pTopLeft,
						pMidTop,
						pBottomLeft,
						pTopRight,
						pBottomRight);
				case 5 : // left and top patches higher LOD
					return CreateIndices(
						pTopLeft,
						pMidTop,
						pMidLeft,
						pTopRight,
						pBottomLeft,
						pBottomRight);
				case 6 : // right and top patches higher LOD
					return CreateIndices(
						pBottomLeft,
						pTopLeft,
						pBottomRight,
						pMidTop,
						pMidRight,
						pTopRight);
				case 7 : // left, right and top patches higher LOD
					return CreateIndices(
						pTopLeft,
						pMidTop,
						pMidLeft,
						pTopRight,
						pBottomLeft,
						pMidRight,
						pBottomRight);
				case 8 : // bottom patch higher LOD
					return CreateIndices(
						pBottomLeft,
						pTopLeft,
						pMidBottom,
						pTopRight,
						pBottomRight);
				case 9 : // left and bottom patches higher LOD
					return CreateIndices(
						pBottomLeft,
						pMidLeft,
						pMidBottom,
						pTopLeft,
						pBottomRight,
						pTopRight);
				case 10 : // right and bottom patches higher LOD
					return CreateIndices(
						pTopLeft,
						pTopRight,
						pBottomLeft,
						pMidRight,
						pMidBottom,
						pBottomRight);
				case 11 : // left, right and bottom patches higher LOD
					return CreateIndices(
						pBottomRight,
						pMidBottom,
						pMidRight,
						pBottomLeft,
						pTopRight,
						pMidLeft,
						pTopLeft);
				case 12 : // top and bottom patches higher LOD
					return CreateIndices(
						pTopLeft,
						pMidTop,
						pBottomLeft,
						pTopRight,
						pMidBottom,
						pBottomRight);
				case 13 : // left, top and bottom patches higher LOD
					return CreateIndices(
						pBottomLeft,
						pMidLeft,
						pMidBottom,
						pTopLeft,
						pBottomRight,
						pMidTop,
						pTopRight);
				case 14 : // right, top and bottom patches higher LOD
					return CreateIndices(
						pTopRight,
						pMidRight,
						pMidTop,
						pBottomRight,
						pTopLeft,
						pMidBottom,
						pBottomLeft);
				case 15 : // all patches higher LOD
					return CreateIndices(
						pTopLeft,
						pMidTop,
						pMidLeft,
						pTopRight,
						pBottomLeft,
						pMidRight,
						pMidBottom,
						pBottomRight);
				default :
					throw new ArgumentException("Impossible to be here");
			}
		}

		private short[] CreateIndices(params short[][] pPositions)
		{
			int length = pPositions.Length;
			short[] pIndices = new short[length];

			// positions are passed in in groups of two
			for (int i = 0; i < length; i++)
			{
				short hX = pPositions[i][0];
				short hY = pPositions[i][1];

				pIndices[i] = _terrain.GetIndex(hX, hY);
			}

			return pIndices;
		}

		#endregion

		/// <summary>
		/// Calculates the absolute difference between fHeightBefore,
		/// and the average of fHeight1 and fHeight2
		/// </summary>
		/// <param name="fHeightBefore"></param>
		/// <param name="fHeight1"></param>
		/// <param name="fHeight2"></param>
		private void CalculateDelta(float fHeightBefore, float fHeight1, float fHeight2)
		{
			float fHeightAfter = (fHeight1 + fHeight2) / 2.0f;
			float fDelta = Math.Abs(fHeightBefore - fHeightAfter);
			if (fDelta > m_fMaximumDelta)
			{
				m_fMaximumDelta = fDelta;
			}
		}

		private static short GetPowerOfTwo(short hNumber)
		{
			switch (hNumber)
			{
				case 0 :
					return 1;
				case 1 :
					return 2;
				case 2 :
					return 4;
				case 3 :
					return 8;
				case 4 :
					return 16;
				case 5 :
					return 32;
				case 6 :
					return 64;
				case 7 :
					return 128;
				case 8 :
					return 256;
				default :
					return -1;
			}
		}

		#endregion

		#endregion
	}
}
