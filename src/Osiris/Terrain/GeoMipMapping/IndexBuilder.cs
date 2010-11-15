using System;

namespace Osiris.Terrain.GeoMipMapping
{
	/// <summary>
	/// Summary description for IndexBuilder.
	/// </summary>
	public class IndexBuilder
	{
		#region Fields

		private GeoMipMappedTerrain _terrain;

		private int _maxIndices;

		/// <summary>
		/// We declare this static to avoid declaring many very large arrays.
		/// However, this means that only one instance of this class can be in
		/// use at any one time.
		/// </summary>
		private static short[] m_pIndices;

		private int m_nCurrentPosition;
		private short m_hLastIndexAdded;

		#endregion

		#region Properties

		public short[] Indices
		{
			get
			{
				// copy static array into correctly sized array
				short[] pIndices = new short[m_nCurrentPosition];
				Array.Copy(m_pIndices, pIndices, m_nCurrentPosition);
				return pIndices;
			}
		}

		#endregion

		#region Constructor

		public IndexBuilder(GeoMipMappedTerrain terrain)
		{
			_terrain = terrain;

			if (m_pIndices == null)
			{
				_maxIndices = terrain.PatchSize * terrain.PatchSize * 5;
				m_pIndices = new short[_maxIndices];
			}

			m_nCurrentPosition = 0;
		}

		#endregion

		#region Methods

		public void AddIndex(int nX, int nY)
		{
			// get index
			m_hLastIndexAdded = _terrain.GetIndex(nX, nY);

			// store
			m_pIndices[m_nCurrentPosition++] = m_hLastIndexAdded;
		}

		public void AddLastIndexAgain()
		{
			m_pIndices[m_nCurrentPosition++] = m_hLastIndexAdded;
		}

		#endregion
	}
}
