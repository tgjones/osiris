using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics;

namespace Osiris.Terrain.GeoClipMapping
{
	public class RingFixups : NestedDrawableGameComponent
	{
		#region Fields

		private static VertexBuffer m_pSharedVertexBuffer;
		private static IndexBuffer m_pSharedIndexBuffer;

		private readonly int m_nGridSpacing;

		private Vector4 m_tScaleFactor;

		#endregion

		public RingFixups(Game game, int nGridSpacing)
			: base(game)
		{
			m_nGridSpacing = nGridSpacing;
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
		/// Creates the single-instance index buffer that will be used by all ring fixups
		/// </summary>
		/// <param name="pGraphicsDevice"></param>
		private void CreateSharedIndexBuffer()
		{
			// TODO: may want to use triangle lists insead of triangle strips to avoid the
			// excessive number of degenerate triangles used for top and bottom fix-ups

			// create indices for all 4 ring fix-ups
			short[] pIndices = new short[Settings.RING_FIXUPS_NUM_INDICES];
			int nCounter = 0;

			// left side
			for (short y = 0; y < 2; y++)
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

					// special case for last column
					if (x == Settings.BLOCK_SIZE_M_MINUS_ONE)
					{
						// add index for degenerate triangle
						pIndices[nCounter++] = hIndex2;
					}
				}
			}

			// top side
			for (short y = 0; y < Settings.BLOCK_SIZE_M_MINUS_ONE; y++)
			{
				for (short x = 0; x < 3; x++)
				{
					short hIndex1 = (short) ((y * 3) + x + (Settings.BLOCK_SIZE_M * 3));
					short hIndex2 = (short) ((y * 3) + x + 3 + (Settings.BLOCK_SIZE_M * 3));

					// special case for first column
					if (x == 0)
					{
						// add index for degenerate triangle
						pIndices[nCounter++] = hIndex1;
					}

					// standard indices
					pIndices[nCounter++] = hIndex1;
					pIndices[nCounter++] = hIndex2;

					// special case for last column
					if (x == 2)
					{
						// add index for degenerate triangle
						pIndices[nCounter++] = hIndex2;
					}
				}
			}

			// right side
			for (short y = 0; y < 2; y++)
			{
				for (short x = 0; x < Settings.BLOCK_SIZE_M; x++)
				{
					short hIndex1 = (short) ((y * Settings.BLOCK_SIZE_M) + x + (Settings.BLOCK_SIZE_M * 3 * 2));
					short hIndex2 = (short) ((y * Settings.BLOCK_SIZE_M) + x + Settings.BLOCK_SIZE_M + (Settings.BLOCK_SIZE_M * 3 * 2));

					// special case for first column
					if (x == 0)
					{
						// add index for degenerate triangle
						pIndices[nCounter++] = hIndex1;
					}

					// standard indices
					pIndices[nCounter++] = hIndex1;
					pIndices[nCounter++] = hIndex2;

					// special case for last column
					if (x == Settings.BLOCK_SIZE_M_MINUS_ONE)
					{
						// add index for degenerate triangle
						pIndices[nCounter++] = hIndex2;
					}
				}
			}

			// bottom side
			for (short y = 0; y < Settings.BLOCK_SIZE_M_MINUS_ONE; y++)
			{
				for (short x = 0; x < 3; x++)
				{
					short hIndex1 = (short) ((y * 3) + x + (Settings.BLOCK_SIZE_M * 3 * 3));
					short hIndex2 = (short) ((y * 3) + x + 3 + (Settings.BLOCK_SIZE_M * 3 * 3));

					// special case for first column
					if (x == 0)
					{
						// add index for degenerate triangle
						pIndices[nCounter++] = hIndex1;
					}

					// standard indices
					pIndices[nCounter++] = hIndex1;
					pIndices[nCounter++] = hIndex2;

					// special case for last column in rows that aren't the last row
					if (y < Settings.BLOCK_SIZE_M - 2 && x == 2)
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
				Settings.RING_FIXUPS_NUM_INDICES,
				BufferUsage.None);

			m_pSharedIndexBuffer.SetData<short>(pIndices);
		}

		/// <summary>
		/// Creates the vertex buffer which includes all four ring fixup vertices.
		/// There is a fixup on each side of the level, in the middle.
		/// </summary>
		/// <param name="pGraphicsDevice"></param>
		private void CreateSharedVertexBuffer()
		{
			// create vertices
			TerrainVertex.Shared[] pVertices = new TerrainVertex.Shared[Settings.RING_FIXUPS_NUM_VERTICES];

			int nCounter = 0;

			// left side
			for (short y = Settings.BLOCK_SIZE_M_MINUS_ONE * 2; y < (Settings.BLOCK_SIZE_M_MINUS_ONE * 2) + 3; y++)
			{
				for (short x = 0; x < Settings.BLOCK_SIZE_M; x++)
				{
					// write shared data
					pVertices[nCounter++] = new TerrainVertex.Shared(x, y);
				}
			}

			// top side
			for (short y = Settings.GRID_SIZE_N_MINUS_ONE - Settings.BLOCK_SIZE_M_MINUS_ONE; y < Settings.GRID_SIZE_N; y++)
			{
				for (short x = Settings.BLOCK_SIZE_M_MINUS_ONE * 2; x < (Settings.BLOCK_SIZE_M_MINUS_ONE * 2) + 3; x++)
				{
					// write shared data
					pVertices[nCounter++] = new TerrainVertex.Shared(x, y);
				}
			}

			// right side
			for (short y = Settings.BLOCK_SIZE_M_MINUS_ONE * 2; y < (Settings.BLOCK_SIZE_M_MINUS_ONE * 2) + 3; y++)
			{
				for (short x = Settings.GRID_SIZE_N_MINUS_ONE - Settings.BLOCK_SIZE_M_MINUS_ONE; x < Settings.GRID_SIZE_N; x++)
				{
					// write shared data
					pVertices[nCounter++] = new TerrainVertex.Shared(x, y);
				}
			}

			// bottom side
			for (short y = 0; y < Settings.BLOCK_SIZE_M; y++)
			{
				for (short x = Settings.BLOCK_SIZE_M_MINUS_ONE * 2; x < (Settings.BLOCK_SIZE_M_MINUS_ONE * 2) + 3; x++)
				{
					// write shared data
					pVertices[nCounter++] = new TerrainVertex.Shared(x, y);
				}
			}

			// create shared vertex buffer
			m_pSharedVertexBuffer = new VertexBuffer(
				GraphicsDevice,
				typeof(TerrainVertex.Shared),
				Settings.RING_FIXUPS_NUM_VERTICES,
				BufferUsage.None);

			m_pSharedVertexBuffer.SetData<TerrainVertex.Shared>(pVertices);
		}

		#endregion

		public void Update(Vector2 tLevelMinPosition)
		{
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
				m_pSharedVertexBuffer,
				0,
				TerrainVertex.Shared.SizeInBytes);
			GraphicsDevice.Indices = m_pSharedIndexBuffer;

			// set offset (scaling should be done in level)
			pEffect.SetValue("ScaleFactor", m_tScaleFactor);

			pEffect.Begin();
			foreach (EffectPass pEffectPass in pEffect.CurrentTechnique.Passes)
			{
				pEffectPass.Begin();

				// render
				GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip,
				0,                                   // base vertex
				0,                                   // min vertex index
				Settings.RING_FIXUPS_NUM_VERTICES,    // total num vertices - note that is NOT just vertices that are indexed, but all vertices
				0,                                   // start index
				Settings.RING_FIXUPS_NUM_PRIMITIVES); // primitive count

				pEffectPass.End();
			}
			pEffect.End();
		}

		#endregion
	}
}
