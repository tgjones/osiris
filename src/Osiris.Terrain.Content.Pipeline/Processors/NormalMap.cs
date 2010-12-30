using Microsoft.Xna.Framework;
using Osiris.Terrain.Content.Pipeline.Graphics;

namespace Osiris.Terrain.Content.Pipeline.Processors
{
	/// <summary>
	/// Summary description for NormalMap.
	/// </summary>
	public class NormalMap
	{
		private readonly HeightMapContent _heightMap;

		#region Fields

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
			get { return m_pNormals[nX, nY]; }
		}

		#endregion

		#region Constructor

		public NormalMap(HeightMapContent heightMap)
		{
			_heightMap = heightMap;
			m_nWidth = heightMap.Width;
			m_nHeight = heightMap.Height;

			m_pNormals = new Vector3[m_nWidth, m_nHeight];
			for (int x = 0; x < m_nWidth; ++x)
				for (int y = 0; y < m_nHeight; ++y)
					m_pNormals[x, y] = CalculateNormal(x, y);
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

			float h0 = _heightMap[x, y];
			float h1 = _heightMap[x, y + 1];
			float h2 = _heightMap[x + 1, y];
			float h3 = _heightMap[x + 1, y + 1];

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

		#endregion
	}
}