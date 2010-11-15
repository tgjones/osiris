using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics.Cameras;
using Osiris.Sky;

namespace Osiris.Terrain.GeoMipMapping
{
	/// <summary>
	/// Summary description for Patch.
	/// </summary>
	public class Patch : Osiris.DrawableGameComponent
	{
		#region Variables

		private GeoMipMappedTerrain _terrain;
		private bool _oneTimeInitialised = false;

		private IHeightMapService _heightMap;
		private int _patchOffsetX, _patchOffsetY;

		private static VertexBuffer m_pSharedVertexBuffer;

		private static short m_hNumLevels;
		private static int m_nNumVertices;

		private VertexBuffer m_pLocalVertexBuffer;

		private Patch m_pLeft, m_pRight, m_pTop, m_pBottom;

		private Level[] m_pLevels;
		private int m_nActiveLevel;

		private Vector3 m_tCentre;
		private Vector2 m_tOffset;

		private BoundingBox _boundingBox;

		private bool _visible;

		#endregion

		#region Properties

		public new bool Visible
		{
			get { return _visible; }
			set { _visible = value; }
		}

		public static VertexBuffer SharedVertexBuffer
		{
			get {return m_pSharedVertexBuffer;}
		}

		public static short NumLevels
		{
			get {return m_hNumLevels;}
		}

		public static int NumVertices
		{
			get {return m_nNumVertices;}
		}

		public int ActiveLevel
		{
			get {return m_nActiveLevel;}
			set {m_nActiveLevel = value;}
		}

		private bool LeftMoreDetailed
		{
			get {return (m_pLeft != null && m_pLeft.ActiveLevel < m_nActiveLevel);}
		}

		private bool RightMoreDetailed
		{
			get {return (m_pRight != null && m_pRight.ActiveLevel < m_nActiveLevel);}
		}

		private bool TopMoreDetailed
		{
			get {return (m_pTop != null && m_pTop.ActiveLevel < m_nActiveLevel);}
		}

		private bool BottomMoreDetailed
		{
			get {return (m_pBottom != null && m_pBottom.ActiveLevel < m_nActiveLevel);}
		}

		public int LeftActiveLevel
		{
			get {return (m_pLeft != null) ? m_pLeft.ActiveLevel : 1000;}
		}

		public int RightActiveLevel
		{
			get {return (m_pRight != null) ? m_pRight.ActiveLevel : 1000;}
		}

		public int TopActiveLevel
		{
			get {return (m_pTop != null) ? m_pTop.ActiveLevel : 1000;}
		}

		public int BottomActiveLevel
		{
			get {return (m_pBottom != null) ? m_pBottom.ActiveLevel : 1000;}
		}

		public Vector2 Offset
		{
			get {return m_tOffset;}
		}

		public override BoundingBox BoundingBox
		{
			get { return _boundingBox; }
		}

		#endregion

		#region Constructors

		public Patch(Game game, GeoMipMappedTerrain terrain, IHeightMapService pHeightMap, int nPatchOffsetX, int nPatchOffsetY)
			: base(game, null, false, false, true)
		{
			_terrain = terrain;

			if (!_oneTimeInitialised)
			{
				// In the static constructor, we first construct indices,
				// since they're fixed for each patch size.
				// Then we fix the vertex definition for terrain vertices
				
				// TODO: i think numlevels is log2, or something like that

				// calculate number of levels, based on patch size
				int nCurrent = (terrain.PatchSize - 1) * 2;
				m_hNumLevels = 0;
				while (nCurrent != 1)
				{
					nCurrent /= 2;
					m_hNumLevels++;
				}

				m_nNumVertices = terrain.PatchSize * terrain.PatchSize;

				_oneTimeInitialised = true;
			}

			_heightMap = pHeightMap;
			_patchOffsetX = nPatchOffsetX;
			_patchOffsetY = nPatchOffsetY;
		}

		#endregion

		#region Methods

		protected override void LoadContent()
		{
			base.LoadContent();

			#region Shared vertex buffer

			if (m_pSharedVertexBuffer == null)
			{
				// create shared vertex buffer
				m_pSharedVertexBuffer = new VertexBuffer(
					this.GraphicsDevice,
					typeof(TerrainVertex.Shared),
					m_nNumVertices,
					BufferUsage.None);

				TerrainVertex.Shared[] sharedData = new TerrainVertex.Shared[_terrain.PatchSize * _terrain.PatchSize];
				for (int y = 0; y < _terrain.PatchSize; y++)
					for (int x = 0; x < _terrain.PatchSize; x++)
						sharedData[(y * _terrain.PatchSize) + x] = new TerrainVertex.Shared(x, y);

				m_pSharedVertexBuffer.SetData<TerrainVertex.Shared>(sharedData);
			}

			#endregion

			#region Local vertex buffer

			// create local vertex buffer for patch
			m_pLocalVertexBuffer = new VertexBuffer(
				this.GraphicsDevice,
				typeof(TerrainVertex.Local),
				m_nNumVertices,
				BufferUsage.None);

			// fill vertex buffer
			TerrainVertex.Local[] localVertices = new TerrainVertex.Local[_terrain.PatchSize * _terrain.PatchSize];

			int nStartX = _patchOffsetX * (_terrain.PatchSize - 1);
			int nStartY = _patchOffsetY * (_terrain.PatchSize - 1);
			int nEndX = nStartX + _terrain.PatchSize;
			int nEndY = nStartY + _terrain.PatchSize;

			IAtmosphereService atmosphere = GetService<IAtmosphereService>();
			float fMinZ = float.MaxValue, fMaxZ = float.MinValue;
			int index = 0;
			for (int y = nStartY; y < nEndY; y++)
			{
				for (int x = nStartX; x < nEndX; x++)
				{
					// write local data
					float fZ = _heightMap[x, y] + atmosphere.EarthRadius;

					if (fZ < fMinZ) fMinZ = fZ;
					if (fZ > fMaxZ) fMaxZ = fZ;

					localVertices[index++] = new TerrainVertex.Local(fZ);
				}
			}

			m_pLocalVertexBuffer.SetData<TerrainVertex.Local>(localVertices);

			#endregion

			m_pLevels = new Level[m_hNumLevels];
			for (int i = 0; i < m_hNumLevels; i++)
				m_pLevels[i] = new Level(this.Game, _terrain, this.GraphicsDevice, i, _heightMap, nStartX, nEndX, nStartY, nEndY);

			#region Bounding box, centre, and offset

			_boundingBox = new BoundingBox(
				new Vector3(nStartX, fMinZ, nStartY),
				new Vector3(nEndX, fMaxZ, nEndY));

			float fAverageZ = (fMinZ + fMaxZ) / 2.0f;

			m_tCentre = new Vector3(
				nStartX + ((_terrain.PatchSize - 1) / 2.0f),
				fAverageZ,
				nStartY + ((_terrain.PatchSize - 1) / 2.0f));

			m_tOffset = new Vector2(
				_patchOffsetX * (_terrain.PatchSize - 1),
				_patchOffsetY * (_terrain.PatchSize - 1));

			#endregion
		}

		public void RecalculateMinimumD(float tau)
		{
			for (int i = 0; i < m_hNumLevels; i++)
				m_pLevels[i].RecalculateMinimumD(tau, GetService<ICameraService>(), this.GraphicsDevice);
		}

		public void SetNeighbours(Patch pLeft, Patch pRight, Patch pTop, Patch pBottom)
		{
			m_pLeft = pLeft;
			m_pRight = pRight;
			m_pTop = pTop;
			m_pBottom = pBottom;
		}

		public static int GetNeighboursCode(bool bLeft, bool bRight, bool bTop, bool bBottom)
		{
			return ((bLeft) ? 1 : 0) | ((bRight) ? 2 : 0) | ((bTop) ? 4 : 0) | ((bBottom) ? 8 : 0);
		}

		public static void GetNeighboursBoolean(int nCode, out bool bLeft, out bool bRight, out bool bTop, out bool bBottom)
		{
			bLeft = (nCode & 1) == 1;
			bRight = (nCode & 2) == 2;
			bTop = (nCode & 4) == 4;
			bBottom = (nCode & 8) == 8;
		}

		public void UpdateLOD()
		{
			// calculate distance(sq) from centre of this patch to the camera
			ICameraService camera = GetService<ICameraService>();
			Vector3 tDistance = m_tCentre - camera.Position;
			float fDistanceSq = tDistance.LengthSquared();

			// choose which level to use
			m_nActiveLevel = 0;
			for (short i = 1; i < m_hNumLevels; i++)
				if (fDistanceSq > m_pLevels[i].MinimumDSq)
					m_nActiveLevel = i;
		}

		public void UpdateTessellation()
		{
			// work out bitmask for neighbours
			int nCode = GetNeighboursCode(LeftMoreDetailed, RightMoreDetailed, TopMoreDetailed, BottomMoreDetailed);
			m_pLevels[m_nActiveLevel].NeighboursCode = nCode;
		}

		protected override void DrawComponent(GameTime gameTime, bool shadowPass)
		{
			this.GraphicsDevice.Vertices[1].SetSource(m_pLocalVertexBuffer, 0, TerrainVertex.Local.SizeInBytes);
			m_pLevels[m_nActiveLevel].Draw(this.GraphicsDevice);
		}

		#endregion
	}
}
