using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics;

namespace Osiris.Terrain.GeoClipMapping
{
	public class InteriorTrim : NestedDrawableGameComponent
	{
		#region Fields

		private static VertexBuffer[] m_pSharedVertexBuffers;
		private static IndexBuffer m_pSharedIndexBuffer;

		private WhichInteriorTrim m_eActiveInteriorTrim;

		private readonly int m_nGridSpacing;

		private Vector4 m_tScaleFactor;

		#endregion

		#region Properties

		public WhichInteriorTrim ActiveInteriorTrim
		{
			get { return m_eActiveInteriorTrim; }
			set { m_eActiveInteriorTrim = value; }
		}

		public Vector2 CoarserGridPosMin
		{
			get
			{
				// calculate min and max position of level, in coordinates of coarser level grid
				switch (m_eActiveInteriorTrim)
				{
					case InteriorTrim.WhichInteriorTrim.BottomLeft:
						return new Vector2(Settings.BLOCK_SIZE_M, Settings.BLOCK_SIZE_M);
					case InteriorTrim.WhichInteriorTrim.BottomRight:
						return new Vector2(Settings.BLOCK_SIZE_M_MINUS_ONE, Settings.BLOCK_SIZE_M);
					case InteriorTrim.WhichInteriorTrim.TopLeft:
						return new Vector2(Settings.BLOCK_SIZE_M, Settings.BLOCK_SIZE_M_MINUS_ONE);
					case InteriorTrim.WhichInteriorTrim.TopRight:
						return new Vector2(Settings.BLOCK_SIZE_M_MINUS_ONE);
					default :
						return Vector2.Zero;
				}
			}
		}

		#endregion

		#region Constructors

		public InteriorTrim(Game game, int nGridSpacing)
			: base(game)
		{
			m_nGridSpacing = nGridSpacing;
		}

		#endregion

		#region Start methods

		protected override void LoadContent()
		{
			// TODO: It would be good to compare the performance of triangle list and strips.

			// only create shared buffers once
			if (m_pSharedIndexBuffer == null)
				CreateSharedIndexBuffer();

			if (m_pSharedVertexBuffers == null)
				CreateSharedVertexBuffers();
		}

		private void CreateSharedVertexBuffers()
		{
			m_pSharedVertexBuffers = new VertexBuffer[4];

			CreateSharedVertexBuffer(WhichInteriorTrim.TopLeft);
			CreateSharedVertexBuffer(WhichInteriorTrim.TopRight);
			CreateSharedVertexBuffer(WhichInteriorTrim.BottomLeft);
			CreateSharedVertexBuffer(WhichInteriorTrim.BottomRight);
		}

		/// <summary>
		/// Creates the index buffer for all interior trim. The reason we can use one
		/// index buffer for all four vertex buffers is that we always draw the horizontal
		/// section first, then the vertical section, and we use the same orientation.
		/// This means we have to use two surplus vertices to get everything to match up.
		/// </summary>
		/// <param name="pGraphicsDevice"></param>
		private void CreateSharedIndexBuffer()
		{
			short[] pIndices = new short[Settings.INTERIOR_TRIM_NUM_INDICES];
			int nCounter = 0;

			// first, horizontal section
			for (short x = 0; x < (2 * Settings.BLOCK_SIZE_M) + 1; x++)
			{
				short hIndex1 = (short) x;
				short hIndex2 = (short) (x + (2 * Settings.BLOCK_SIZE_M) + 1);

				// standard indices
				pIndices[nCounter++] = hIndex1;
				pIndices[nCounter++] = hIndex2;

				// special case for last column
				if (x == 2 * Settings.BLOCK_SIZE_M)
				{
					// add index for degenerate triangle
					pIndices[nCounter++] = hIndex2;
				}
			}

			// second, vertical section
			for (short y = 0; y < (2 * Settings.BLOCK_SIZE_M) - 1; y++)
			{
				for (short x = 0; x < 2; x++)
				{
					short hIndex1 = (short) ((y * 2) + (4 * Settings.BLOCK_SIZE_M) + 2 + x);
					short hIndex2 = (short) ((y * 2) + (4 * Settings.BLOCK_SIZE_M) + 4 + x);

					// add degenerate index for left side
					if (x == 0)
					{
						pIndices[nCounter++] = hIndex1;
					}

					// standard indices
					pIndices[nCounter++] = hIndex1;
					pIndices[nCounter++] = hIndex2;

					// add degenerate index for right side, unless we're at the end
					if (x == 1 && y < (2 * Settings.BLOCK_SIZE_M) - 2)
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
				Settings.INTERIOR_TRIM_NUM_INDICES,
				BufferUsage.None);

			m_pSharedIndexBuffer.SetData<short>(pIndices);
		}

		#region Create shared vertex buffers

		/// <summary>
		/// Creates the vertex buffer for the top left interior trim
		/// </summary>
		/// <param name="pGraphicsDevice"></param>
		private void PopulateVerticesTopLeft(TerrainVertex.Shared[] pVertices)
		{
			int nCounter = 0;

			// top segment
			for (short y = Settings.GRID_SIZE_N - Settings.BLOCK_SIZE_M - 1; y < Settings.GRID_SIZE_N - Settings.BLOCK_SIZE_M_MINUS_ONE; y++)
			{
				for (short x = Settings.BLOCK_SIZE_M_MINUS_ONE; x < Settings.GRID_SIZE_N - Settings.BLOCK_SIZE_M_MINUS_ONE; x++)
				{
					// write shared data
					pVertices[nCounter++] = new TerrainVertex.Shared(x, y);
				}
			}

			// left segment
			for (short y = Settings.BLOCK_SIZE_M_MINUS_ONE; y < (3 * Settings.BLOCK_SIZE_M) - 1; y++)
			{
				for (short x = Settings.BLOCK_SIZE_M_MINUS_ONE; x < Settings.BLOCK_SIZE_M + 1; x++)
				{
					// write shared data
					pVertices[nCounter++] = new TerrainVertex.Shared(x, y);
				}
			}
		}

		/// <summary>
		/// Creates the vertex buffer for the top right interior trim
		/// </summary>
		/// <param name="pGraphicsDevice"></param>
		private void PopulateVerticesTopRight(TerrainVertex.Shared[] pVertices)
		{
			int nCounter = 0;

			// top segment
			for (short y = Settings.GRID_SIZE_N - Settings.BLOCK_SIZE_M - 1; y < Settings.GRID_SIZE_N - Settings.BLOCK_SIZE_M_MINUS_ONE; y++)
			{
				for (short x = Settings.BLOCK_SIZE_M_MINUS_ONE; x < Settings.GRID_SIZE_N - Settings.BLOCK_SIZE_M_MINUS_ONE; x++)
				{
					// write shared data
					pVertices[nCounter++] = new TerrainVertex.Shared(x, y);
				}
			}

			// right segment
			for (short y = Settings.BLOCK_SIZE_M_MINUS_ONE; y < (3 * Settings.BLOCK_SIZE_M) - 1; y++)
			{
				for (short x = Settings.GRID_SIZE_N - Settings.BLOCK_SIZE_M - 1; x < Settings.GRID_SIZE_N - Settings.BLOCK_SIZE_M_MINUS_ONE; x++)
				{
					// write shared data
					pVertices[nCounter++] = new TerrainVertex.Shared(x, y);
				}
			}
		}

		/// <summary>
		/// Creates the vertex buffer for the bottom left interior trim
		/// </summary>
		/// <param name="pGraphicsDevice"></param>
		private void PopulateVerticesBottomLeft(TerrainVertex.Shared[] pVertices)
		{
			int nCounter = 0;

			// bottom segment
			for (short y = Settings.BLOCK_SIZE_M_MINUS_ONE; y < Settings.BLOCK_SIZE_M + 1; y++)
			{
				for (short x = Settings.BLOCK_SIZE_M_MINUS_ONE; x < Settings.GRID_SIZE_N - Settings.BLOCK_SIZE_M_MINUS_ONE; x++)
				{
					// write shared data
					pVertices[nCounter++] = new TerrainVertex.Shared(x, y);
				}
			}

			// left segment
			for (short y = Settings.BLOCK_SIZE_M; y < (3 * Settings.BLOCK_SIZE_M); y++)
			{
				for (short x = Settings.BLOCK_SIZE_M_MINUS_ONE; x < Settings.BLOCK_SIZE_M + 1; x++)
				{
					// write shared data
					pVertices[nCounter++] = new TerrainVertex.Shared(x, y);
				}
			}
		}

		/// <summary>
		/// Creates the vertex buffer for the bottom right interior trim
		/// </summary>
		/// <param name="pGraphicsDevice"></param>
		private void PopulateVerticesBottomRight(TerrainVertex.Shared[] pVertices)
		{
			int nCounter = 0;

			// bottom segment
			for (short y = Settings.BLOCK_SIZE_M_MINUS_ONE; y < Settings.BLOCK_SIZE_M + 1; y++)
			{
				for (short x = Settings.BLOCK_SIZE_M_MINUS_ONE; x < Settings.GRID_SIZE_N - Settings.BLOCK_SIZE_M_MINUS_ONE; x++)
				{
					// write shared data
					pVertices[nCounter++] = new TerrainVertex.Shared(x, y);
				}
			}

			// right segment
			for (short y = Settings.BLOCK_SIZE_M; y < (3 * Settings.BLOCK_SIZE_M); y++)
			{
				for (short x = Settings.GRID_SIZE_N - Settings.BLOCK_SIZE_M - 1; x < Settings.GRID_SIZE_N - Settings.BLOCK_SIZE_M_MINUS_ONE; x++)
				{
					// write shared data
					pVertices[nCounter++] = new TerrainVertex.Shared(x, y);
				}
			}
		}

		private void CreateSharedVertexBuffer(WhichInteriorTrim eWhichInteriorTrim)
		{
			// create vertices
			TerrainVertex.Shared[] pVertices = new TerrainVertex.Shared[Settings.INTERIOR_TRIM_NUM_VERTICES];

			switch (eWhichInteriorTrim)
			{
				case WhichInteriorTrim.TopLeft:
					PopulateVerticesTopLeft(pVertices);
					break;
				case WhichInteriorTrim.TopRight:
					PopulateVerticesTopRight(pVertices);
					break;
				case WhichInteriorTrim.BottomLeft:
					PopulateVerticesBottomLeft(pVertices);
					break;
				case WhichInteriorTrim.BottomRight:
					PopulateVerticesBottomRight(pVertices);
					break;
			}

			// create shared vertex buffer
			m_pSharedVertexBuffers[(int) eWhichInteriorTrim] = new VertexBuffer(
				GraphicsDevice,
				typeof(TerrainVertex.Shared),
				Settings.INTERIOR_TRIM_NUM_VERTICES,
				BufferUsage.None);

			m_pSharedVertexBuffers[(int) eWhichInteriorTrim].SetData<TerrainVertex.Shared>(pVertices);
		}

		#endregion

		#endregion

		public void Update(Vector2 tLevelMinPosition, IntVector2 tViewerPosGridCoords)
		{
			// update interior trim position based on viewer position
			if (tViewerPosGridCoords.X == Settings.CentralSquareMax.X && tViewerPosGridCoords.Y == Settings.CentralSquareMin.Y)
				m_eActiveInteriorTrim = WhichInteriorTrim.TopLeft;
			else if (tViewerPosGridCoords.X == Settings.CentralSquareMin.X && tViewerPosGridCoords.Y == Settings.CentralSquareMin.Y)
				m_eActiveInteriorTrim = WhichInteriorTrim.TopRight;
			else if (tViewerPosGridCoords.X == Settings.CentralSquareMax.X && tViewerPosGridCoords.Y == Settings.CentralSquareMax.Y)
				m_eActiveInteriorTrim = WhichInteriorTrim.BottomLeft;
			else if (tViewerPosGridCoords.X == Settings.CentralSquareMin.X && tViewerPosGridCoords.Y == Settings.CentralSquareMax.Y)
				m_eActiveInteriorTrim = WhichInteriorTrim.BottomRight;

			Vector2 tWorldPosition = tLevelMinPosition;

			m_tScaleFactor = new Vector4(
				m_nGridSpacing,
				m_nGridSpacing,
				tWorldPosition.X,
				tWorldPosition.Y);
		}

		#region Draw methods

		public override void Draw(ExtendedEffect pEffect)
		{
			GraphicsDevice.Vertices[0].SetSource(
				m_pSharedVertexBuffers[(int) m_eActiveInteriorTrim],
				0,
				TerrainVertex.Shared.SizeInBytes);
			GraphicsDevice.Indices = m_pSharedIndexBuffer;

			// set offset (scaling should be done in level)
			pEffect.SetValue("ScaleFactor", m_tScaleFactor);

			pEffect.Begin();
			foreach (EffectPass pEffectPass in pEffect.CurrentTechnique.Passes)
			{
				pEffectPass.Begin();

				GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip,
					0,                                      // base vertex
					0,                                      // min vertex index
					Settings.INTERIOR_TRIM_NUM_VERTICES,    // total num vertices - note that is NOT just vertices that are indexed, but all vertices
					0,                                      // start index
					Settings.INTERIOR_TRIM_NUM_PRIMITIVES); // primitive count

				pEffectPass.End();
			}
			pEffect.End();
		}

		#endregion

		#region Enums

		public enum WhichInteriorTrim
		{
			TopLeft     = 0,
			TopRight    = 1,
			BottomLeft  = 2,
			BottomRight = 3
		}

		#endregion
	}
}
