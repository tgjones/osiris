using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Osiris.Graphics.Cameras;

namespace Osiris.Terrain.Simple
{
	public class SimpleTerrain : Terrain
	{
		private VertexDeclaration _vertexDeclaration;
		private VertexBuffer _vertexBuffer;
		private IndexBuffer _indexBuffer;
		private string _textureAssetName;
		private Texture2D _texture;

		private int _numVertices;
		private int _numIndices;

		public override BoundingBox BoundingBox
		{
			get
			{
				IHeightMapService terrainData = GetService<IHeightMapService>();
				return new BoundingBox(new Vector3(0, 0, -terrainData.Width), new Vector3(terrainData.Height, 1, 0));
			}
		}

		public SimpleTerrain(Game game, string textureAssetName)
			: base(game, @"Terrain\Simple\SimpleTerrain")
		{
			_textureAssetName = textureAssetName;
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			IHeightMapService terrainData = GetService<IHeightMapService>();
			_numVertices = terrainData.Width * terrainData.Height;

			int numInternalRows = terrainData.Height - 2;
			_numIndices = (2 * terrainData.Width * (1 + numInternalRows)) + (2 * numInternalRows);

			VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[_numVertices];
			for (int z = 0; z < terrainData.Height; z++)
			{
				for (int x = 0; x < terrainData.Width; x++)
				{
					float height = terrainData[x, z];

					vertices[GetIndex(terrainData.Height, x, z)] = new VertexPositionNormalTexture(
						new Vector3(x, height, -z), new Vector3(0, 1, 0),
						new Vector2(x / (float)(terrainData.Width - 1), z / (float)(terrainData.Height - 1)));
				}
			}

			_vertexBuffer = new VertexBuffer(
				this.GraphicsDevice,
				typeof(VertexPositionNormalTexture),
				vertices.Length,
				BufferUsage.WriteOnly);
			_vertexBuffer.SetData<VertexPositionNormalTexture>(vertices);

			short[] indices = new short[_numIndices]; int indexCounter = 0;
			for (int z = 0; z < terrainData.Height - 1; z++)
			{
				// insert index for degenerate triangle
				if (z > 0)
					indices[indexCounter++] = GetIndex(terrainData.Height, 0, z);

				for (int x = 0; x < terrainData.Width; x++)
				{
					indices[indexCounter++] = GetIndex(terrainData.Height, x, z);
					indices[indexCounter++] = GetIndex(terrainData.Height, x, z + 1);
				}

				// insert index for degenerate triangle
				if (z < terrainData.Height - 2)
					indices[indexCounter++] = GetIndex(terrainData.Height, terrainData.Width - 1, z);
			}

			_indexBuffer = new IndexBuffer(
				this.GraphicsDevice,
				typeof(short),
				indices.Length,
				BufferUsage.WriteOnly);
			_indexBuffer.SetData<short>(indices);

			_vertexDeclaration = new VertexDeclaration(
				this.GraphicsDevice, VertexPositionNormalTexture.VertexElements);

			ContentManager content = (ContentManager)this.Game.Services.GetService(typeof(ContentManager));
			_texture = content.Load<Texture2D>(_textureAssetName);

			_effect.SetValue("GrassTexture", _texture);
			_effect.SetValue("TerrainWidth", terrainData.Width);
			_effect.SetValue("TerrainHeight", terrainData.Height);
		}

		private short GetIndex(int height, int x, int z)
		{
			return (short) ((z * height) + x);
		}

		protected override void DrawComponent(GameTime gameTime, bool shadowPass)
		{
			//this.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;
			this.GraphicsDevice.VertexDeclaration = _vertexDeclaration;
			this.GraphicsDevice.Vertices[0].SetSource(_vertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
			this.GraphicsDevice.Indices = _indexBuffer;

			_effect.Begin();
			foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
			{
				pass.Begin();

				this.GraphicsDevice.DrawIndexedPrimitives(
					PrimitiveType.TriangleStrip,
					0,
					0,
					_numVertices,
					0,
					_numIndices - 2);

				pass.End();
			}
			_effect.End();
		}

		/*public override float GetHeight(float x, float z)
		{
			int integerX = MathsHelper.FloorToInt(x);
			int integerZ = MathsHelper.FloorToInt(z);
			float fractionalX = x - integerX;
			float fractionalZ = z - integerZ;

			float v1 = GetHeight(integerX, integerZ);
			float v2 = GetHeight(integerX + 1, integerZ);
			float v3 = GetHeight(integerX, integerZ + 1);
			float v4 = GetHeight(integerX + 1, integerZ + 1);

			float i1 = PerlinNoise.Interpolate(v1, v2, fractionalX);
			float i2 = PerlinNoise.Interpolate(v3, v4, fractionalX);

			return PerlinNoise.Interpolate(i1, i2, fractionalZ);
		}*/

	}
}
