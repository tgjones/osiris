using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Osiris.Terrain.ChunkedLod
{
	public class ChunkedLodTerrain : DrawableGameComponent
	{
		private int _chunkSize;

		private int _vertexCount, _primitiveCount;

		private VertexDeclaration _vertexDeclaration;
		private VertexBuffer _vertexBuffer;
		private IndexBuffer _indexBuffer;
		private Chunk _rootChunk;

		public override BoundingBox BoundingBox
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		public ChunkedLodTerrain(Game game)
			: base(game, @"Terrain\ChunkedLod\Terrain", false, false, false)
		{
			IsAlwaysVisible = true;
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			IHeightMapService heightMap = GetService<IHeightMapService>();

			// chunk size must be: n = 2^k - 3
			// k > 2
			_chunkSize = 5;

			// n is also the number of subdivisions

			_vertexCount = _chunkSize * _chunkSize;
			TerrainVertex.Shared[] vertices = new TerrainVertex.Shared[_vertexCount];

			for (short z = 0; z < _chunkSize; z++)
				for (short x = 0; x < _chunkSize; x++)
					vertices[GetIndex(x, z)] = new TerrainVertex.Shared(x, z);

			int numInternalRows = _chunkSize - 2;
			int numIndices = (2 * _chunkSize * (1 + numInternalRows)) + (2 * numInternalRows);
			_primitiveCount = numIndices - 2;
			short[] indices = new short[numIndices]; int indexCounter = 0;
			for (short z = 0; z < _chunkSize - 1; z++)
			{
				// insert index for degenerate triangle
				if (z > 0)
					indices[indexCounter++] = GetIndex(0, z);

				for (short x = 0; x < _chunkSize; x++)
				{
					indices[indexCounter++] = GetIndex(x, z);
					indices[indexCounter++] = GetIndex(x, (short) (z + 1));
				}

				// insert index for degenerate triangle
				if (z < _chunkSize - 2)
					indices[indexCounter++] = GetIndex((short) (_chunkSize - 1), z);
			}

			// create shared vertex buffer
			_vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(TerrainVertex.Shared), _vertexCount, BufferUsage.WriteOnly);
			_vertexBuffer.SetData<TerrainVertex.Shared>(vertices);

			// create shared index buffer
			_indexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), numIndices, BufferUsage.WriteOnly);
			_indexBuffer.SetData<short>(indices);

			// create vertex declaration
			_vertexDeclaration = new VertexDeclaration(GraphicsDevice, TerrainVertex.VertexElements);

			// create dummy elevation texture
			Texture2D elevationTexture = new Texture2D(GraphicsDevice, _chunkSize, _chunkSize, 1, TextureUsage.None, SurfaceFormat.Single);
			float[] heights = new float[_vertexCount];
			for (int i = 0; i < _vertexCount; i++)
				heights[i] = i % _chunkSize;
			elevationTexture.SetData<float>(heights);
			_effect.SetValue("ElevationTexture", elevationTexture);
			_effect.SetValue("ElevationTextureSize", _chunkSize);

			// calculate scaling of root node - scale is same number of LODs
			_rootChunk = new Chunk(2, Vector2.Zero);

			_rootChunk.Children.Add(new Chunk(1, new Vector2(0, 0)));
			_rootChunk.Children.Add(new Chunk(1, new Vector2(_chunkSize, 0)));
			_rootChunk.Children.Add(new Chunk(1, new Vector2(0, _chunkSize)));
			_rootChunk.Children.Add(new Chunk(1, new Vector2(_chunkSize, _chunkSize)));
		}

		private short GetIndex(short x, short z)
		{
			return (short) ((z * _chunkSize) + x);
		}

		protected override void UpdateComponent(GameTime gameTime)
		{
			base.UpdateComponent(gameTime);

			_rootChunk.Update();
		}

		protected override void DrawComponent(GameTime gameTime)
		{
			GraphicsDevice.VertexDeclaration = _vertexDeclaration;
			GraphicsDevice.Vertices[0].SetSource(_vertexBuffer, 0, TerrainVertex.Shared.SizeInBytes);
			GraphicsDevice.Indices = _indexBuffer;

			DrawChunkRecursive(_rootChunk);
		}

		private void DrawChunkRecursive(Chunk chunk)
		{
			if (chunk.ShouldDraw || chunk.Children.Count == 0) // if this is the correct LOD, or else we are at the finest level
			{
				DrawChunk(chunk);
			}
			else // draw children
			{
				foreach (Chunk childChunk in chunk.Children)
					DrawChunkRecursive(childChunk);
			}
		}

		/// <summary>
		/// Called by Chunk class
		/// </summary>
		public void DrawChunk(Chunk chunk)
		{
			_effect.SetValue("ChunkOffset", chunk.Offset);
			_effect.SetValue("ChunkScale", chunk.Scale);

			_effect.Begin();
			foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
			{
				pass.Begin();
				GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, _vertexCount, 0, _primitiveCount);
				pass.End();
			}
			_effect.End();
		}
	}
}
