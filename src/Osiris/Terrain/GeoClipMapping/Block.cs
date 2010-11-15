using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics;

namespace Osiris.Terrain.GeoClipMapping
{
	public class Block : NestedDrawableGameComponent
	{
		#region Fields

		private static VertexBuffer m_pSharedVertexBuffer;
		private static IndexBuffer m_pSharedIndexBuffer;

		private readonly int m_nGridSpacing;
		private readonly Vector4 m_tFineBlockOrig;
		private readonly Vector2 m_tFineBlockOrig2;
		private readonly Vector2 m_tOffset;

		private Vector4 m_tScaleFactor;

		private Vector3 m_tPositionMin, m_tPositionMax;

		private bool m_bVisible;

		#endregion

		#region Properties

		public static VertexBuffer SharedVertexBuffer
		{
			get { return m_pSharedVertexBuffer; }
		}

		public static IndexBuffer SharedIndexBuffer
		{
			get { return m_pSharedIndexBuffer; }
		}

		public BoundingBox BoundingBox
		{
			get { return new BoundingBox(m_tPositionMin, m_tPositionMax); }
		}

		public bool Visible
		{
			set { m_bVisible = value; }
		}

		#endregion

		public Block(Game game, int nGridSpacing, Vector2 tGridSpaceOffset, Vector2 tOffset)
			: base(game)
		{
			m_nGridSpacing = nGridSpacing;
			m_tFineBlockOrig = new Vector4(Settings.ELEVATION_TEXTURE_SIZE_INVERSE, Settings.ELEVATION_TEXTURE_SIZE_INVERSE,
				tGridSpaceOffset.X / Settings.ELEVATION_TEXTURE_SIZE, tGridSpaceOffset.Y / Settings.ELEVATION_TEXTURE_SIZE);
			m_tFineBlockOrig2 = new Vector2(tGridSpaceOffset.X / Settings.GRID_SIZE_N, tGridSpaceOffset.Y / Settings.GRID_SIZE_N);
			m_tOffset = tOffset;
		}

		#region Start methods

		protected override void LoadContent()
		{
			// only create shared buffers once
			if (m_pSharedIndexBuffer == null)
				CreateSharedIndexBuffer();

			if (m_pSharedVertexBuffer == null)
				CreateSharedVertexBuffer();
		}

		/// <summary>
		/// Creates the single-instance index buffer that will be used by all blocks
		/// </summary>
		/// <param name="pGraphicsDevice"></param>
		private void CreateSharedIndexBuffer()
		{
			// TODO: may be able to optimise this by re-ordering the vertices such that
			// the vertex cache is used better. see
			// http://www.gamedev.net/community/forums/mod/journal/journal.asp?jn=316777&reply_id=2588637

			// create indices for an m x m block, including indices for degenerate triangles
			short[] pIndices = new short[Settings.BLOCK_NUM_INDICES];
			int nCounter = 0;
			for (short y = 0; y < Settings.BLOCK_SIZE_M_MINUS_ONE; y++)
			{
				for (short x = 0; x < Settings.BLOCK_SIZE_M; x++)
				{
					short hIndex1 = (short) ((y * Settings.BLOCK_SIZE_M) + x);
					short hIndex2 = (short) ((y * Settings.BLOCK_SIZE_M) + x + Settings.BLOCK_SIZE_M);
					
					// special case for first column in rows that aren't the first row
					if (y > 0 && x == 0)
					{
						// add index for degenerate triangle
						pIndices[nCounter++] = hIndex1;
					}

					// standard indices
					pIndices[nCounter++] = hIndex1;
					pIndices[nCounter++] = hIndex2;

					// special case for last column in rows that aren't the last row
					if (y < Settings.BLOCK_SIZE_M - 2 && x == Settings.BLOCK_SIZE_M_MINUS_ONE)
					{
						// add index for degenerate triangle
						pIndices[nCounter++] = hIndex2;
					}
				}
			}

			// create shared index buffer
			m_pSharedIndexBuffer = new IndexBuffer(
				GraphicsDevice,
				typeof(short),
				Settings.BLOCK_NUM_INDICES,
				BufferUsage.None);

			m_pSharedIndexBuffer.SetData<short>(pIndices);
		}

		private void CreateSharedVertexBuffer()
		{
			// create vertices
			TerrainVertex.Shared[] pVertices = new TerrainVertex.Shared[Settings.BLOCK_NUM_VERTICES];

			int nCounter = 0;
			for (short y = 0; y < Settings.BLOCK_SIZE_M; y++)
			{
				for (short x = 0; x < Settings.BLOCK_SIZE_M; x++)
				{
					// write shared data
					pVertices[nCounter++] = new TerrainVertex.Shared(x, y);
				}
			}

			// create shared vertex buffer
			m_pSharedVertexBuffer = new VertexBuffer(
				GraphicsDevice,
				typeof(TerrainVertex.Shared),
				Settings.BLOCK_NUM_VERTICES,
				BufferUsage.None);

			m_pSharedVertexBuffer.SetData<TerrainVertex.Shared>(pVertices);
		}

		#endregion

		public void Update(Vector2 tLevelMinPosition)
		{
			Vector2 tWorldPosition = tLevelMinPosition + m_tOffset;

			m_tScaleFactor = new Vector4(
				m_nGridSpacing,
				m_nGridSpacing,
				tWorldPosition.X,
				tWorldPosition.Y);

			m_tPositionMin = new Vector3(tWorldPosition.X, -20000.0f, tWorldPosition.Y);

			int nBlockSizeWorldSpace = Settings.BLOCK_SIZE_M_MINUS_ONE * m_nGridSpacing;
			m_tPositionMax = new Vector3(tWorldPosition.X + nBlockSizeWorldSpace, 20000.0f, tWorldPosition.Y + nBlockSizeWorldSpace);
		}

		#region Draw methods

		public override void Draw(ExtendedEffect pEffect)
		{
			if (m_bVisible)
			{
				pEffect.SetValue("FineBlockOrig", m_tFineBlockOrig);
				pEffect.SetValue("FineBlockOrig2", m_tFineBlockOrig2);
				pEffect.SetValue("ScaleFactor", m_tScaleFactor);
				pEffect.CommitChanges();

				// render
				GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip,
					0,                              // base vertex
					0,                              // min vertex index
					Settings.BLOCK_NUM_VERTICES,    // total num vertices - note that is NOT just vertices that are indexed, but all vertices
					0,                              // start index
					Settings.BLOCK_NUM_PRIMITIVES); // primitive count
			}
		}

		#endregion
	}
}
