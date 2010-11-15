using System;
using Microsoft.Xna.Framework;

namespace Osiris.Terrain
{
	/// <summary>
	/// Summary description for NormalMap.
	/// </summary>
	public class NormalMap : GameComponent, ITerrainNormalsService
	{
		#region Fields

		private IHeightMapService _terrainData;
		private int m_nWidth;
		private int m_nHeight;
		private Vector3[,] m_pNormals;

		#endregion

		#region Properties

		/// <summary>
		/// Retrieves the normal for the specified coordinates
		/// </summary>
		public Vector3 this[int nX, int nY]
		{
			get {return m_pNormals[nX, nY];}
		}

		#endregion

		#region Constructor

		public NormalMap(Game game)
			: base(game)
		{
			game.Services.AddService(typeof(ITerrainNormalsService), this);

			_terrainData = (IHeightMapService) game.Services.GetService(typeof(IHeightMapService));
			m_nWidth = _terrainData.Width;
			m_nHeight = _terrainData.Height;

			m_pNormals = new Vector3[m_nWidth, m_nHeight];
			for (int x = 0; x < m_nWidth; ++x)
				for (int y = 0; y < m_nHeight; ++y)
					m_pNormals[x, y] = CalculateNormal(x, y);

			/*

			m_pNormals = new Vector3[m_nWidth, m_nHeight];
			for (int y = 1; y < m_nHeight - 1; y++)
			{
				for (int x = 1; x < m_nWidth - 1; x++)
				{
					// calculate normal
					m_pNormals[x, y] = CalculateNormal(
						pHeightMap.GetPosition(x - 1, y - 1),
						pHeightMap.GetPosition(x - 0, y - 1),
						pHeightMap.GetPosition(x + 1, y - 1),
						pHeightMap.GetPosition(x - 1, y - 0),
						pHeightMap.GetPosition(x - 0, y - 0),
						pHeightMap.GetPosition(x + 1, y - 0),
						pHeightMap.GetPosition(x - 1, y + 1),
						pHeightMap.GetPosition(x - 0, y + 1),
						pHeightMap.GetPosition(x + 1, y + 1));
				}
			}*/
		}

		#endregion

		#region Methods

		/// <summary>
		/// Function computes the normal for the xy'th quad.
		/// We take the quad normal as the average of the two
		/// triangles that make up the quad.
		/// 
		///       u
		/// h0*-------*h1
		///   |      /|
		///  v|     / |t
		///   |    /  |
		///   |   /   |
		/// h2*-------*h3
		///       s
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private Vector3 CalculateQuadNormal(int x, int y)
		{
			const int CELL_SPACING = 1;

			float h0 = _terrainData[x,     y];
			float h1 = _terrainData[x, y + 1];
			float h2 = _terrainData[x + 1, y];
			float h3 = _terrainData[x + 1, y + 1];

			Vector3 u = new Vector3(CELL_SPACING, h1 - h0, 0);
			Vector3 v = new Vector3(0, h2 - h0, -CELL_SPACING);

			Vector3 s = new Vector3(-CELL_SPACING, h2 - h3, 0);
			Vector3 t = new Vector3(0, h1 - h3, CELL_SPACING);

			Vector3 n1 = Vector3.Cross(u, v);
			n1.Normalize();

			Vector3 n2 = Vector3.Cross(s, t);
			n2.Normalize();

			return (n1 + n2) * 0.5f;
		}

		/// <summary>
		/// The vertex normal is found by averaging the normals of the four quads that surround the vertex
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private Vector3 CalculateNormal(int x, int y)
		{
			Vector3 avg = Vector3.Zero;
			float num = 0;

			for (int m = x - 1; m <= x; ++m)
			{
				for (int n = y - 1; n <= y; ++n)
				{
					// vertices on heightmap boundaries do not have
					// surrounding quads in some directions, so we just
					// average in a normal vector that is axis aligned
					// with the y-axis.
					if (m < 0 || n < 0 || m == m_nWidth - 1 || n == m_nHeight - 1)
					{
						avg += Vector3.Up;
						num += 1.0f;
					}
					else
					{
						avg += CalculateQuadNormal(m, n);
						num += 1.0f;
					}
				}
			}
			avg /= num;

			avg.Normalize();
			return avg;
		}

		/*public Vector3 CalculateNormal(float fMapX, float fMapY)
		{
			// convert coordinates to heightmap scale
			fMapX *= m_nWidth - 1;
			fMapY *= m_nHeight - 1;

			// calculate integer and fractional parts of coordinates
			int nIntX0 = MathsHelper.FloorToInt(fMapX);
			int nIntY0 = MathsHelper.FloorToInt(fMapY);
			float fFractionalX = fMapX - nIntX0;
			float fFractionalY = fMapY - nIntY0;

			// get coordinates for "other" side of quad
			int nIntX1 = MathsHelper.Clamp(nIntX0 + 1, 0, m_nWidth - 1);
			int nIntY1 = MathsHelper.Clamp(nIntY0 + 1, 0, m_nHeight - 1);

			// read normals for vertices
			Vector3 t0 = m_pNormals[nIntX0, nIntY0];
			Vector3 t1 = m_pNormals[nIntX1, nIntY0];
			Vector3 t2 = m_pNormals[nIntX0, nIntY1];
			Vector3 t3 = m_pNormals[nIntX1, nIntY1];

			// average the results
			Vector3 tAverageLo = (t1 * fFractionalX) + (t0 * (1.0f - fFractionalX));
			Vector3 tAverageHi = (t3 * fFractionalX) + (t2 * (1.0f - fFractionalX));

			// calculate normal
			Vector3 tNormal = (tAverageHi * fFractionalY) + (tAverageLo * (1.0f - fFractionalY));

			// renormalise
			tNormal.Normalize();

			return tNormal;
		}

		private Vector3 CalculateNormal(
			Vector3 tVertex0,
			Vector3 tVertex1,
			Vector3 tVertex2,
			Vector3 tVertex3,
			Vector3 tVertex4,
			Vector3 tVertex5,
			Vector3 tVertex6,
			Vector3 tVertex7,
			Vector3 tVertex8)
		{
			// calculate face normals
			Vector3 tFace0 = Vector3.Cross(tVertex3 - tVertex4, tVertex0 - tVertex4);
			Vector3 tFace1 = Vector3.Cross(tVertex0 - tVertex4, tVertex1 - tVertex4);
			Vector3 tFace2 = Vector3.Cross(tVertex1 - tVertex4, tVertex2 - tVertex4);
			Vector3 tFace3 = Vector3.Cross(tVertex2 - tVertex4, tVertex5 - tVertex4);
			Vector3 tFace4 = Vector3.Cross(tVertex5 - tVertex4, tVertex8 - tVertex4);
			Vector3 tFace5 = Vector3.Cross(tVertex8 - tVertex4, tVertex7 - tVertex4);
			Vector3 tFace6 = Vector3.Cross(tVertex7 - tVertex4, tVertex6 - tVertex4);
			Vector3 tFace7 = Vector3.Cross(tVertex6 - tVertex4, tVertex3 - tVertex4);

			tFace0.Normalize();
			tFace1.Normalize();
			tFace2.Normalize();
			tFace3.Normalize();
			tFace4.Normalize();
			tFace5.Normalize();
			tFace6.Normalize();
			tFace7.Normalize();

			// add face normals
			Vector3 tNormal = tFace0 + tFace1 + tFace2 + tFace3
				+ tFace4 + tFace5 + tFace6 + tFace7;

			// normalise vector
			tNormal.Normalize();

			// return vertex normal
			return tNormal;
		}*/

		#endregion
	}
}
