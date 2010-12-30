using System;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Osiris.Terrain.Content.Pipeline.Graphics;

namespace Osiris.Terrain.Content.Pipeline.Processors
{
	public class LevelContentBuilder
	{
		private const int TotalNeighbourCodes = 16;

		private readonly HeightMapContent _heightMap;
		private readonly int _patchSize;
		private readonly int _numLevels;
		private readonly int _level;
		private readonly int _startX;
		private readonly int _endX;
		private readonly int _startY;
		private readonly int _endY;

		private float _maximumDelta;

		public LevelContentBuilder(HeightMapContent heightMap, int patchSize, int numLevels, int level, int startX, int endX, int startY, int endY)
		{
			_heightMap = heightMap;
			_patchSize = patchSize;
			_numLevels = numLevels;
			_level = level;
			_startX = startX;
			_endX = endX;
			_startY = startY;
			_endY = endY;
		}

		public LevelContent Build()
		{
			if (_level > 0) // don't need to calculate for top level, since it's "minimum d" is always 0
			{
				int hSkip = GetPowerOfTwo(_level);
				int hHalfSkip = hSkip / 2;
				for (int y = _startY; y < _endY - 1; y += hSkip)
				{
					for (int x = _startX; x < _endX; x += hSkip)
					{
						// down-left
						if (x > _startX)
							CalculateDelta(_heightMap[x - hHalfSkip, y + hHalfSkip], _heightMap[x, y], _heightMap[x - hSkip, y + hSkip]);

						// down
						CalculateDelta(_heightMap[x, y + hHalfSkip], _heightMap[x, y], _heightMap[x, y + hSkip]);

						// down-right
						if (x < _endX - 1)
							CalculateDelta(_heightMap[x + hHalfSkip, y + hHalfSkip], _heightMap[x, y], _heightMap[x + hSkip, y + hSkip]);

						// right
						if (x < _endX - 1)
							CalculateDelta(_heightMap[x + hHalfSkip, y], _heightMap[x, y], _heightMap[x + hSkip, y]);
					}
				}
			}

			IndexCollection[] indices = BuildIndices(_numLevels, _level);

			return new LevelContent
			{
				Indices = indices,
				MaximumDelta = _maximumDelta
			};
		}

		private IndexCollection[] BuildIndices(int numLevels, int level)
		{
			// calculate number of vertices
			int nNumVerticesOneSide = GetPowerOfTwo((int) (numLevels - 1 - level)) + 1;
			int nNumVertices = nNumVerticesOneSide * nNumVerticesOneSide;

			// calculate number of primitives
			int nNumRows = nNumVerticesOneSide - 1;

			// for level 0, we only have set of indices because we never adapt to neighbours
			int nTotalNeighbourCodes = (level == 0) ? 1 : TotalNeighbourCodes;
			IndexCollection[] indices = new IndexCollection[nTotalNeighbourCodes];

			int hSkip = GetPowerOfTwo(level);
			int hHalfSkip = hSkip / 2;

			for (int j = 0; j < nTotalNeighbourCodes; j++)
			{
				bool bLeft, bRight, bTop, bBottom;
				GetNeighboursBoolean(j, out bLeft, out bRight, out bTop, out bBottom);

				// generate indices
				if (level == numLevels - 1)
				{
					indices[j] = AddHighestLevelTriangles(hSkip, hHalfSkip, j);
				}
				else
				{
					IndexBuilder pIndexBuilder = new IndexBuilder(this);
					for (int y = 0; y < _patchSize - hSkip; y += hSkip)
						AddTriangleRow(pIndexBuilder, y, hSkip, hHalfSkip, bLeft, bRight, bTop, bBottom);
					indices[j] = pIndexBuilder.Indices;
				}
			}

			return indices;
		}

		private static void GetNeighboursBoolean(int nCode, out bool bLeft, out bool bRight, out bool bTop, out bool bBottom)
		{
			bLeft = (nCode & 1) == 1;
			bRight = (nCode & 2) == 2;
			bTop = (nCode & 4) == 4;
			bBottom = (nCode & 8) == 8;
		}

		#region AddTriangleRow method

		private void AddTriangleRow(IndexBuilder pIndexBuilder,
			int nY, int hSkip, int hHalfSkip,
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
			if (bBottom && (nY == _patchSize - 1 - hSkip))
			{
				AddTrianglesBottomRow(pIndexBuilder, nY, hSkip, hHalfSkip, bLeft, bRight);
				return;
			}

			int nStartX = 0, nEndX = _patchSize;

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
				int nRightX = _patchSize - 1 - hSkip;
				pIndexBuilder.AddIndex(nRightX, nY + hSkip);
				pIndexBuilder.AddLastIndexAgain();
				pIndexBuilder.AddIndex(nRightX + hSkip, nY + hSkip);
				pIndexBuilder.AddIndex(nRightX, nY);
				pIndexBuilder.AddIndex(nRightX + hSkip, nY + hHalfSkip);
				pIndexBuilder.AddIndex(nRightX + hSkip, nY);
			}

			// if this is not last row, add extra index for degenerate triangle
			if (nY != _patchSize - 1 - hSkip)
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
		private void AddTrianglesTopRow(IndexBuilder pIndexBuilder, int hSkip, int hHalfSkip,
			bool bLeft, bool bRight)
		{
			int nStartX = 0, nEndX = _patchSize - 1;

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
				if (x != _patchSize - 1)
				{
					pIndexBuilder.AddLastIndexAgain();
				}
				if (x == _patchSize - hSkip - 1)
				{
					pIndexBuilder.AddLastIndexAgain();
				}
			}

			if (bRight)
			{
				int nRightX = _patchSize - 1 - hSkip;
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
			int nY, int hSkip, int hHalfSkip,
			bool bLeft, bool bRight)
		{
			int nStartX = 0, nEndX = _patchSize - 1;

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
				if (x != _patchSize - 1 - hSkip)
				{
					pIndexBuilder.AddLastIndexAgain();
				}
			}

			if (bRight)
			{
				int nRightX = _patchSize - 1 - hSkip;
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
		private IndexCollection AddHighestLevelTriangles(int hSkip, int hHalfSkip, int nCode)
		{
			// declare helper variables, for clarification (not the fastest, but I'd rather it was clear)
			int[] pTopLeft = new int[] { 0, 0 };
			int[] pTopRight = new int[] { hSkip, 0 };
			int[] pBottomLeft = new int[] { 0, hSkip };
			int[] pBottomRight = new int[] { hSkip, hSkip };
			int[] pMidLeft = new int[] { 0, hHalfSkip };
			int[] pMidRight = new int[] { hSkip, hHalfSkip };
			int[] pMidTop = new int[] { hHalfSkip, 0 };
			int[] pMidBottom = new int[] { hHalfSkip, hSkip };

			switch (nCode)
			{
				case 0: // all patches same LOD
					return CreateIndices(
						pTopLeft,
						pTopRight,
						pBottomLeft,
						pBottomRight);
				case 1: // left patch higher LOD
					return CreateIndices(
						pTopLeft,
						pTopRight,
						pMidLeft,
						pBottomRight,
						pBottomLeft);
				case 2: // right patch higher LOD
					return CreateIndices(
						pTopLeft,
						pTopRight,
						pBottomLeft,
						pMidRight,
						pBottomRight);
				case 3: // left and right patches higher LOD
					return CreateIndices(
						pTopLeft,
						pTopRight,
						pMidLeft,
						pMidRight,
						pBottomLeft,
						pBottomRight);
				case 4: // top patch higher LOD
					return CreateIndices(
						pTopLeft,
						pMidTop,
						pBottomLeft,
						pTopRight,
						pBottomRight);
				case 5: // left and top patches higher LOD
					return CreateIndices(
						pTopLeft,
						pMidTop,
						pMidLeft,
						pTopRight,
						pBottomLeft,
						pBottomRight);
				case 6: // right and top patches higher LOD
					return CreateIndices(
						pBottomLeft,
						pTopLeft,
						pBottomRight,
						pMidTop,
						pMidRight,
						pTopRight);
				case 7: // left, right and top patches higher LOD
					return CreateIndices(
						pTopLeft,
						pMidTop,
						pMidLeft,
						pTopRight,
						pBottomLeft,
						pMidRight,
						pBottomRight);
				case 8: // bottom patch higher LOD
					return CreateIndices(
						pBottomLeft,
						pTopLeft,
						pMidBottom,
						pTopRight,
						pBottomRight);
				case 9: // left and bottom patches higher LOD
					return CreateIndices(
						pBottomLeft,
						pMidLeft,
						pMidBottom,
						pTopLeft,
						pBottomRight,
						pTopRight);
				case 10: // right and bottom patches higher LOD
					return CreateIndices(
						pTopLeft,
						pTopRight,
						pBottomLeft,
						pMidRight,
						pMidBottom,
						pBottomRight);
				case 11: // left, right and bottom patches higher LOD
					return CreateIndices(
						pBottomRight,
						pMidBottom,
						pMidRight,
						pBottomLeft,
						pTopRight,
						pMidLeft,
						pTopLeft);
				case 12: // top and bottom patches higher LOD
					return CreateIndices(
						pTopLeft,
						pMidTop,
						pBottomLeft,
						pTopRight,
						pMidBottom,
						pBottomRight);
				case 13: // left, top and bottom patches higher LOD
					return CreateIndices(
						pBottomLeft,
						pMidLeft,
						pMidBottom,
						pTopLeft,
						pBottomRight,
						pMidTop,
						pTopRight);
				case 14: // right, top and bottom patches higher LOD
					return CreateIndices(
						pTopRight,
						pMidRight,
						pMidTop,
						pBottomRight,
						pTopLeft,
						pMidBottom,
						pBottomLeft);
				case 15: // all patches higher LOD
					return CreateIndices(
						pTopLeft,
						pMidTop,
						pMidLeft,
						pTopRight,
						pBottomLeft,
						pMidRight,
						pMidBottom,
						pBottomRight);
				default:
					throw new ArgumentException("Impossible to be here");
			}
		}

		private IndexCollection CreateIndices(params int[][] pPositions)
		{
			IndexCollection indices = new IndexCollection();
			int length = pPositions.Length;

			// positions are passed in in groups of two
			for (int i = 0; i < length; i++)
			{
				int hX = pPositions[i][0];
				int hY = pPositions[i][1];

				indices.Add(GetIndex(hX, hY));
			}

			return indices;
		}

		internal int GetIndex(int nX, int nY)
		{
			// sanity check that x and y are valid
			System.Diagnostics.Debug.Assert(nX < _patchSize && nY < _patchSize);
			return (nY * _patchSize) + nX;
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
			if (fDelta > _maximumDelta)
				_maximumDelta = fDelta;
		}

		private static int GetPowerOfTwo(int number)
		{
			switch (number)
			{
				case 0:
					return 1;
				case 1:
					return 2;
				case 2:
					return 4;
				case 3:
					return 8;
				case 4:
					return 16;
				case 5:
					return 32;
				case 6:
					return 64;
				case 7:
					return 128;
				case 8:
					return 256;
				default:
					return -1;
			}
		}
	}
}